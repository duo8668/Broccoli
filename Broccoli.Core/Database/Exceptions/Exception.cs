using System;
using System.Linq.Expressions;

namespace Broccoli.Core.Database.Exceptions
{

    /**
     * Will be thrown when an Expression Visitor, visits an unknown operator.
     *
     * ```cs
     * 	var converter = new PredicateConverter();
     * 	Expression<Func<TModel, bool>> expression = e => e.Id + 1;
     *
     * 	try
     * 	{
     * 		converter.Visit(expression.Body);
     * 	}
     * 	catch (UnknownOperatorException e)
     * 	{
     * 		// This will be "ExpressionType.Add".
     * 		e.UnknownOperator;
     * 	}
     * ```
     */
    public class UnknownOperatorException : Exception
    {
        public ExpressionType UnknownOperator { get; protected set; }

        public UnknownOperatorException(ExpressionType unknownOperator)
        : base("We don't know what to do with the Operator: " + unknownOperator.ToString())
        {
            this.UnknownOperator = unknownOperator;
        }
    }

    /**
     * Will be thrown when an Expression Visitor, gives up basically.
     *
     * Without using the expensive DynamicInvoke it is actually rather complex
     * to extract the actual values referenced by an expression. In the case
     * the Expression Visitor can not extract such a value it will throw this.
     *
     * Instead of using something like this:
     * ```cs
     * 	var value = Some.Other.Complex.Object.That.We.Cant.Decompose.Value;
     * 	var foos = Foo.Where(e => e.Bar > value).ToList();
     * ```
     *
     * You will be able to do get the same end result, all be it without the
     * type safety provided by the expression:
     * ```cs
     * 	var value = Some.Other.Complex.Object.That.We.Cant.Decompose.Value;
     * 	var foos = Foo.Where("Bar > {0}", value).ToList();
     * ```
     */
    public class ExpressionTooComplexException : Exception
    {
        public ExpressionTooComplexException()
        : base("This expression is too complex to decompose and convert into SQL. Consider using the equivalent string.format method.")
        { }
    }
}
