using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using PlasticLang.Ast;
using PlasticLang.Contexts;

namespace PlasticLang.Visitors
{
    public static class Evaluator
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ValueTask<object> Eval(this Syntax syn, PlasticContext context) =>
            syn switch
            {
                NumberLiteral numberLiteral => EvalNumberLiteral(context, numberLiteral),
                StringLiteral str           => EvalStringLiteral(context, str),
                Symbol symbol               => EvalSymbol(context, symbol),
                ListValue listValue         => EvalListValue(context, listValue),
                ArrayValue arrayValue       => EvalArray(context, arrayValue),
                Statements statements       => EvalStatements(context, statements),
                TupleValue tupleValue       => EvalTupleValue(context, tupleValue),
            };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static async ValueTask<object> EvalTupleValue(PlasticContext context, TupleValue tupleValue)
        {
            var items = new object[tupleValue.Items.Length];
            for (var i = 0; i < tupleValue.Items.Length; i++)
            {
                var v = await Eval(tupleValue.Items[i], context);
                items[i] = v;
            }

            var res = new TupleInstance(items);
            return res;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static async ValueTask<object> EvalStatements(PlasticContext context, Statements statements)
        {
            ValueTask<object> result = default;
            foreach (var statement in statements.Elements) result = Eval(statement, context);
            return await result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ValueTask<object> EvalNumberLiteral(PlasticContext context, NumberLiteral numberLiteral)
        {
            if (context is PlasticContextImpl)
            {
                return new ValueTask<object>(numberLiteral.Value);
            }
            return context.Number(numberLiteral);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ValueTask<object> EvalListValue(PlasticContext context, ListValue listValue)
        {
            return context!.Invoke(listValue.Head!, listValue.Rest!)!;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ValueTask<object> EvalSymbol(PlasticContext context, Symbol symbol)
        {
            var v = context.GetSymbol(symbol);
            return ValueTask.FromResult(v!);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ValueTask<object> EvalStringLiteral(PlasticContext context, StringLiteral str)
        {
            return context.QuotedString(str);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static async ValueTask<object> EvalArray(PlasticContext context, ArrayValue arrayValue)
        {
            var items = new object[arrayValue.Items.Length];
            for (var i = 0; i < arrayValue.Items.Length; i++) items[i] = await arrayValue.Items[i].Eval(context);
            return items;
        }
    }
}