﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PlasticLang.Ast;
using PlasticLang.Contexts;
using PlasticLang.Visitors;
using Sprache;

namespace PlasticLang
{
    public static class Plastic
    {
        private static readonly object Exit = new();

        public static ValueTask<object> Run(string code)
        {
            var context = SetupCoreSymbols();
            var userContext = new PlasticContextImpl(context);
            return Run(code, userContext);
        }
    
        public static ValueTask<object> Run(string code, PlasticContext context)
        {
            var res = PlasticParser.Statements.Parse(code);
            ValueTask<object> result = default;
            foreach (var statement in res.Elements) result = statement.Eval(context);
            return result;
        }

        public static void BootstrapLib(PlasticContext context)
        {
            var lib = File.ReadAllText("core.pla");


            var libCode = PlasticParser.Statements.Parse(lib);
            ValueTask<object> result = default;
            foreach (var statement in libCode.Elements) result = statement.Eval(context);
            var temp = result;
        }

        private static PlasticContext SetupCoreSymbols()
        {
            var context = new PlasticContextImpl();


            async ValueTask<object?> Def(PlasticContext c, Syntax[] a)
            {
                var left = a.Left() as Symbol;
                var right = a.Right();

                var value = await right.Eval(c);
                context.Declare(left.Identity, value);
                return value;
            }


            context.Declare("print", Print);
            context.Declare("while", While);
            context.Declare("each", Each);
            context.Declare("if", If);
            context.Declare("elif", Elif);
            context.Declare("else", Else);
            context.Declare("true", true);
            context.Declare("false", false);
            context.Declare("null", null);
            context.Declare("exit", Exit);
            context.Declare("func", Func);
            context.Declare("mixin", Mixin);
            context.Declare("class", Class);
            context.Declare("using", Using);
            context.Declare("eval", Eval);
            context.Declare("assign", Assign);
            context.Declare("def", Def);
            context.Declare("_add", Add);
            context.Declare("_sub", Sub);
            context.Declare("_mul", Mul);
            context.Declare("_div", Div);
            context.Declare("_div", Div);
            context.Declare("_eq", Eq);
            context.Declare("_neq", Neq);
            context.Declare("_gt", Gt);
            context.Declare("_gteq", GtEq);
            context.Declare("_lt", Lt);
            context.Declare("_lteq", LtEq);
            context.Declare("_band", LogicalAnd);
            context.Declare("_bor", LogicalOr);
            context.Declare("_dot", Dotop);
            context.Declare("_not", Not);
            //context.Declare("ActorSystem", actorSystem);


            BootstrapLib(context);

            return context;
        }

        private static async ValueTask<object?> Not(PlasticContext c, Syntax[] a) => 
            !(bool)await a.Left().Eval(c);

        private static async ValueTask<object?> Dotop(PlasticContext c, Syntax[] a)
        {
            var left = a.Left();
            var right = a.Right();

            var l = await left.Eval(c);

            switch (l)
            {
                case object[] arr:
                {
                    var arrayContext = new ArrayContext(arr, c);
                    return await right.Eval(arrayContext);
                }
                case PlasticObject pobj:
                    return await right.Eval(pobj.Context);
                case Type type:
                {
                    var typeContext = new ClrTypeContext(type, c);
                    return await right.Eval(typeContext);
                }
                default:
                {
                    var objContext = new ClrInstanceContext(l, c);
                    return await right.Eval(objContext);
                }
            }
        }

        private static ValueTask<object?> Mixin(PlasticContext c, Syntax[] a)
        {
            var body = a.Last();

            async ValueTask<object?> PlasticMacro(PlasticContext ctx, Syntax[] args)
            {
                var thisContext = ctx;

                for (var i = 0; i < a.Length - 1; i++)
                {
                    var argName = a[i] as Symbol; //TODO: add support for expressions and partial appl
                    thisContext.Declare(argName.Identity, await args[i].Eval(ctx));
                }

                await body.Eval(thisContext);

                return null;
            }

            return ValueTask.FromResult<object>( (PlasticMacro) PlasticMacro);
        }

        private static async ValueTask<object?> Print(PlasticContext c, Syntax[] a)
        {
            var obj = await a.First().Eval(c);
            var source = a.Skip(1).ToArray();
            var args = new object[source.Length];
            for (var i = 0; i < source.Length; i++)
            {
                var v = await source[i].Eval(c);
                args[i] = v;
            }

            if (args.Any())
                Console.WriteLine(obj.ToString(), args);
            else
                Console.WriteLine(obj);
            return obj;
        }

        private static async ValueTask<object?> While(PlasticContext c, Syntax[] a)
        {
            var result = Exit;
            var cond = a.Left();
            var body = a.Right();

            while ((bool)await cond.Eval(c)) result = await body.Eval(c);

            return result;
        }

        private static async ValueTask<object?> If(PlasticContext c, Syntax[] a)
        {
            var cond = a.Left();
            var body = a.Right();

            if ((bool)await cond.Eval(c))
            {
                var res = await body.Eval(c);
                if (res == Exit) return null;
                return res;
            }

            return Exit;
        }

        private static async ValueTask<object?> Elif(PlasticContext c, Syntax[] a)
        {
            var last = c["last"];
            if (last != Exit) return last;

            var cond = a.Left();
            var body = a.Right();

            if ((bool)await cond.Eval(c))
            {
                var res = await body.Eval(c);
                if (res == Exit) return null;
                return res;
            }

            return Exit;
        }

        private static async ValueTask<object?> Else(PlasticContext c, Syntax[] a)
        {
            var last = c["last"];
            if (last != Exit) return last;

            var body = a.Left();

            var res = await body.Eval(c);
            if (res == Exit) return null;

            return res;
        }

        private static async ValueTask<object?> Each(PlasticContext c, Syntax[] a)
        {
            var v = a.Left() as Symbol;
            var body = a[2];

            if (!(await a.Right().Eval(c) is IEnumerable enumerable)) return Exit;

            object result = null!;
            foreach (var element in enumerable)
            {
                c.Declare(v.Identity, element);
                result = await body.Eval(c);
            }

            return result;
        }

        private static ValueTask<object> Func(PlasticContext _, Syntax[] a)
        {
            var argsMinusOne = a.Take(a.Length - 1)
                .Select(arg =>
                {
                    var symbol = arg as Symbol;
                    if (symbol != null)
                    {
                        if (!symbol.Identity.StartsWith("@")) return new Argument(symbol.Identity, ArgumentType.Value);
                        return new Argument(symbol.Identity.Substring(1), ArgumentType.Expression);
                    }

                    throw new NotSupportedException();
                })
                .ToArray();
            var body = a.Last();

            async ValueTask<object?> Op(PlasticContext callingContext, Syntax[] args)
            {
                //full application
                if (args.Length >= argsMinusOne.Length)
                {
                    //create context for this invocation
                    var invocationScope = new PlasticContextImpl(callingContext);
                    var arguments = new List<object>();
                    for (var i = 0; i < args.Length; i++)
                    {
                        var arg = argsMinusOne[i];
                        if (arg.Type == ArgumentType.Expression)
                        {
                            //copy args from caller to this context
                            var value = args[i];
                            invocationScope.Declare(arg.Name, value);
                            arguments.Add(value);
                        }
                        else if (arg.Type == ArgumentType.Value)
                        {
                            var value = await args[i].Eval(callingContext);
                            invocationScope.Declare(arg.Name, value);
                            arguments.Add(value);
                        }

                        invocationScope.Declare("args", arguments.ToArray());
                    }

                    var m = await body.Eval(invocationScope);
                    return m;
                }

                //partial application
                var partialArgs = args.ToArray();

                ValueTask<object?> Partial(PlasticContext ctx, Syntax[] pargs) => Op(ctx, partialArgs.Union(pargs).ToArray());

                return ValueTask.FromResult<object>( (PlasticMacro) Partial);
            }

            return ValueTask.FromResult<object>( (PlasticMacro) Op);
        }


        private static ValueTask<object> Class(PlasticContext c, Syntax[] a)
        {
            var body = a.Last();

            async ValueTask<object> PlasticMacro(PlasticContext ctx, Syntax[] args)
            {
                var thisContext = new PlasticContextImpl(c);

                for (var i = 0; i < a.Length - 1; i++)
                {
                    var argName = a[i] as Symbol; //TODO: add support for expressions and partial appl
                    var arg = await args[i].Eval(ctx);
                    thisContext.Declare(argName!.Identity, arg);
                }

                var self = new PlasticObject(thisContext);
                thisContext.Declare("this", self);
                await body.Eval(thisContext);

                return self;
            }

            return ValueTask.FromResult<object>((PlasticMacro) PlasticMacro!);
        }

        private static ValueTask<object> Using(PlasticContext c, Syntax[] a)
        {
            var path = a.First() as StringLiteral;
            var type = Type.GetType(path.Value);
            return ValueTask.FromResult<object>(type);
        }

        private static async ValueTask<object?> Eval(PlasticContext c, Syntax[] a)
        {
            var code = await a.First().Eval(c) as string;
            var res = await Run(code!, c);
            return res;
        }

        private static async ValueTask<object?> Assign(PlasticContext c, Syntax[] a)
        {
            var left = a.Left();
            var right = a.Right();

            var value = await right.Eval(c);

            switch (left)
            {
                case Symbol assignee:
                    c.SetSymbol(assignee,value);
                    break;
                case ListValue dot:
                {
                    var obj = await dot.Rest[0].Eval(c) as PlasticObject;
                    var memberId = dot.Rest[1] as Symbol;
                    obj![memberId!.Identity] = value;
                    break;
                }
            }

            

            if (left is not TupleValue tuple) return value;
            var arr = value as TupleInstance;
            return await Match(tuple, arr!);
            
            async ValueTask<bool> Match(TupleValue t, TupleInstance values)
            {
                if (values == null) return false;

                if (t.Items.Length != values.Items.Length) return false;

                for (var i = 0; i < values.Items.Length; i++)
                {
                    var l = t.Items[i];
                    var r = values.Items[i];

                    switch (l)
                    {
                        //left is symbol, assign a value to it..
                        case Symbol symbol:
                            c[symbol.Identity] = r;
                            break;
                        case TupleValue leftTuple when r is TupleInstance rightTuple:
                        {
                            //right is a sub tuple, recursive match
                            var subMatch = await Match(leftTuple, rightTuple);
                            if (!subMatch) return false;
                            break;
                        }
                        case TupleValue leftTuple:
                            //left is tuple, right is not. just exit
                            return false;
                        default:
                        {
                            //left is a value, compare to right
                            var lv = await l.Eval(c);
                            if (lv is null)
                            {
                                if (r is not null) return false;
                            }
                            else if (!lv.Equals(r))
                            {
                                return false;
                            }

                            break;
                        }
                    }
                }

                return true;
            }
        }

        private static async ValueTask<object?> Add(PlasticContext c, Syntax[] a) => 
            await (dynamic)a.Left().Eval(c) + (dynamic)await a.Right().Eval(c);

        private static async ValueTask<object?> Sub(PlasticContext c, Syntax[] a) => 
            await (dynamic)a.Left().Eval(c) - (dynamic)await a.Right().Eval(c);

        private static async ValueTask<object?> Mul(PlasticContext c, Syntax[] a) => 
            await (dynamic)a.Left().Eval(c) * (dynamic)await a.Right().Eval(c);

        private static async ValueTask<object?> Div(PlasticContext c, Syntax[] a) => 
            await (dynamic)a.Left().Eval(c) / (dynamic)await a.Right().Eval(c);

        private static async ValueTask<object?> Eq(PlasticContext c, Syntax[] a)
        {
            var left = await a.Left().Eval(c);
            var right = await a.Right().Eval(c);

            if (left == null && right == null)
            {
                return true;
            }

            if (left == null || right == null) return false;

            if (left.GetType() != right.GetType()) return false;

            return left == right;
        }

        private static async ValueTask<object?> Neq(PlasticContext c, Syntax[] a) => 
            !((bool)await Eq(c, a))!;

        private static async ValueTask<object?> Gt(PlasticContext c, Syntax[] a) => 
            await (dynamic)a.Left().Eval(c) > (dynamic)await a.Right().Eval(c);

        private static async ValueTask<object?> GtEq(PlasticContext c, Syntax[] a) => 
            await (dynamic)a.Left().Eval(c) >= (dynamic)await a.Right().Eval(c);

        private static async ValueTask<object?> Lt(PlasticContext c, Syntax[] a) => 
            await (dynamic)a.Left().Eval(c) < (dynamic)await a.Right().Eval(c);

        private static async ValueTask<object?> LtEq(PlasticContext c, Syntax[] a) => 
            await (dynamic)a.Left().Eval(c) <= (dynamic)await a.Right().Eval(c);

        private static async ValueTask<object?> LogicalAnd(PlasticContext c, Syntax[] a) => 
            await (dynamic)a.Left().Eval(c) && (dynamic)await a.Right().Eval(c);

        private static async ValueTask<object?> LogicalOr(PlasticContext c, Syntax[] a) => 
            await (dynamic)a.Left().Eval(c) || (dynamic)await a.Right().Eval(c);
    }
}