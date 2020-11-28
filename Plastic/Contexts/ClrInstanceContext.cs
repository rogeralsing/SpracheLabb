using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PlasticLang.Ast;
using PlasticLang.Reflection;
using PlasticLang.Visitors;

namespace PlasticLang.Contexts
{
    public class ClrInstanceContext : PlasticContext
    {
        private readonly object _obj;

        public ClrInstanceContext(object obj, PlasticContext owner) : base(owner)
        {
            _obj = obj;
        }

        public override Cell GetCell(string name)
        {
            throw new NotImplementedException();
        }

        public override object? this[string name]
        {
            get
            {
                var prop = _obj.GetPropertyValue(name);
                var res = prop.GetValue(_obj);
                return res;
            }
            set => throw new NotImplementedException();
        }

        public override async ValueTask<dynamic?> Invoke(Syntax head, Syntax[] args)
        {
            var memberName = "";
            if (head is StringLiteral sl) memberName = sl.Value;


            var methods = _obj.GetType().GetMethods().Where(m => m.Name == memberName).ToArray();
            if (methods.Any())
            {
                var args2 = new List<object>();
                foreach (var a in args)
                {
                    var r = await a.Eval(Parent);
                    args2.Add(r);
                }

                foreach (var method in methods)
                    try
                    {
                        var res = method.Invoke(_obj, args2.ToArray());
                        return ValueTask.FromResult(res);
                    }
                    catch
                    {
                    }
            }

            var properties = _obj.GetType().GetProperties().Where(m => m.Name == memberName).ToArray();
            foreach (var property in properties)
                try
                {
                    var m = property.GetGetMethod()?.Invoke(_obj, null) as PlasticMacro;
                    var res = m(this, args);
                    return res;
                }
                catch
                {
                }

            throw new Exception("No matching method found.");
        }

        public override bool HasProperty(string name) => throw new NotImplementedException();

        public override void Declare(string name, object value) => throw new NotImplementedException();

        public override ValueTask<dynamic> Number(NumberLiteral numberLiteral) => throw new NotImplementedException();

        public override ValueTask<dynamic> QuotedString(StringLiteral stringLiteral)
        {
            var res = this[stringLiteral.Value];
            return ValueTask.FromResult(res);
        }
    }
}