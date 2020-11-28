﻿using System.Threading.Tasks;
using PlasticLang.Ast;

namespace PlasticLang.Contexts
{
    public abstract class PlasticContext
    {
        protected PlasticContext(PlasticContext? parent)
        {
            Parent = parent;
        }

        protected PlasticContext? Parent { get; }

        public abstract object? this[string name] { get; set; }

        public virtual object? GetSymbol(Symbol symbol)
        {
            return this[symbol.Identity];
        }

        public virtual void SetSymbol(Symbol symbol,object value)
        {
            this[symbol.Identity] = value;
        }

        public abstract ValueTask<dynamic> Number(NumberLiteral numberLiteral);
        public abstract ValueTask<dynamic> QuotedString(StringLiteral stringLiteral);
        public abstract ValueTask<dynamic?> Invoke(Syntax head, Syntax[] args);
        public abstract bool HasProperty(string name);
        public abstract void Declare(string name, object value);
        public abstract Cell GetCell(string name);
    }
}