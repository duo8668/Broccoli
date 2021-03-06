﻿using Broccoli.Core.Database.Exceptions;
using Broccoli.Core.Facade;
using Broccoli.Core.Utils;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
namespace Broccoli.Core.Database.Utils.Converters
{
    /**
       * Given an Expression Tree, we will convert it into a SQL WHERE clause.
       *
       * 	Expression<Func<TModel, bool>> expression = m => m.Id == 1;
       *
       * 	var converter = new PredicateConverter();
       * 	converter.Visit(expression.Body);
       *
       * 	// converter.Sql == "Id = {0}"
       * 	// converter.Parameters == new object[] { 1 }
       */
    public class PredicateConverter : ExpressionVisitor, System.IDisposable
    {
        /**
         * The portion of the SQL query that will come after a WHERE clause.
         */
        public string Sql
        {
            get { return this.sql.ToString().Trim(); }
        }

        private StringBuilder sql = new StringBuilder();

        /**
         * A list of parameter values that go along with our sql query segment.
         */
        public object[] Parameters
        {
            get { return this.parameters.ToArray(); }
        }

        private List<object> parameters = new List<object>();

        /**
         * When we recurse into a MemberExpression, looking for a
         * ConstantExpression, we do not want to write anything to
         * the sql StringBuilder.
         */
        private bool blockWriting = false;

        /**
         * In some cases, we need to save the value we get from a MemberInfo
         * and save it for later use, when we are at the correct
         * MemberExpression.
         */
        private object value;

        protected override Expression VisitBinary(BinaryExpression node)
        {
            // Open the binary expression in SQL
            this.sql.Append("(");

            // Go and visit the left hand side of this expression
            this.Visit(node.Left);

            // Add the operator in the middle
            switch (node.NodeType)
            {
                case ExpressionType.Equal:

                    if (node.Right.ToString().Contains("%"))
                    {
                        this.sql.Append("LIKE");
                    }
                    else
                    {
                        this.sql.Append("=");
                    }
                    break;
                case ExpressionType.NotEqual: this.sql.Append("!="); break;
                case ExpressionType.GreaterThan: this.sql.Append(">"); break;
                case ExpressionType.GreaterThanOrEqual: this.sql.Append(">="); break;
                case ExpressionType.LessThan: this.sql.Append("<"); break;
                case ExpressionType.LessThanOrEqual: this.sql.Append("<="); break;

                case ExpressionType.And:
                case ExpressionType.AndAlso:

                    this.sql.Append("AND");

                    break;

                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    this.sql.Append("OR");
                    break;

                default:
                    throw new UnknownOperatorException(node.NodeType);
            }

            // Operator needs a space after it.
            this.sql.Append(" ");

            // Now visit the right hand side of this expression.
            this.Visit(node.Right);

            // Close the binary expression in SQL
            this.sql.Append(") ");

            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            // This will get filled with the "actual" value from our child
            // ConstantExpression if happen to have a child ConstantExpression.
            // see: http://stackoverflow.com/questions/6998523
            object value = null;

            // Recurse down to see if we can simplify...
            this.blockWriting = true;
            var expression = this.Visit(node.Expression);
            this.blockWriting = false;

            // If we've ended up with a constant, and it's a property
            // or a field, we can simplify ourselves to a constant.
            if (expression is ConstantExpression)
            {
                MemberInfo member = node.Member;
                object container = ((ConstantExpression)expression).Value;

                if (member is FieldInfo)
                {
                    value = ((FieldInfo)member).GetValue(container);
                }
                else if (member is PropertyInfo)
                {
                    value = ((PropertyInfo)member).GetValue(container, null);
                }

                // If we managed to actually get a value, lets now create a
                // ConstantExpression with the expected value and Vist it.
                if (value != null)
                {
                    if (value.GetType().IsPrimitive || TypeMapper.IsClrType(value))
                    {
                        this.Visit(Expression.Constant(value));
                    }
                    else
                    {
                        // So if we get to here, what has happened is that
                        // the value returned by the FieldInfo GetValue call
                        // is actually the container, so we save it for later.
                        this.value = value;
                    }
                }
            }
            else if (expression is MemberExpression)
            {
                // Now we can use the value we saved earlier to actually grab the constant value that we expected. I guess this sort of
                // recursion could go on for ages and hence why the accepted answer used DyanmicInvoke. Anyway we will hope that this
                // does the job for our needs.

                MemberInfo member = node.Member;
                object container = this.value;

                if (member is FieldInfo)
                {
                    value = ((FieldInfo)member).GetValue(container);
                }
                else if (member is PropertyInfo)
                {
                    value = ((PropertyInfo)member).GetValue(container, null);
                }

                this.value = null;

                if (value.GetType().IsPrimitive || TypeMapper.IsClrType(value))
                {
                    this.Visit(Expression.Constant(value));
                }
                else
                {
                    throw new ExpressionTooComplexException();
                }
            }

            // TODO: TO map against the column name in the system
            // We only need to do this if we did not have a child ConstantExpression
            if (value == null)
            {
                var typeName = node.Expression.Type.Name;
                var tableName = DbFacade.TableNames[typeName];
                var columnName = DbFacade.ColumnInfos[typeName][node.Member.Name].ColumnName;
                this.sql.Append(string.Concat(tableName, ".", columnName, " "));
            }

            return node;
        }

        protected override Expression VisitConstant(ConstantExpression node)
        {
            if (!this.blockWriting)
            {
                var value = node.Value;

                if (value == null)
                {
                    if (this.sql[this.sql.Length - 2] == '=')
                    {
                        if (this.sql[this.sql.Length - 3] == '!')
                        {
                            this.sql.Remove(this.sql.Length - 3, 3);
                            this.sql.Append("IS NOT NULL");
                        }
                        else
                        {
                            this.sql.Remove(this.sql.Length - 2, 2);
                            this.sql.Append("IS NULL");
                        }
                    }
                }
                else
                {
                    this.sql.Append("@");
                    this.sql.Append(this.parameters.Count);
                    this.sql.Append("");
                    this.parameters.Add(node.Value);
                }
            }

            return node;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
                sql.Clear();
                parameters.Clear();
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~PredicateConverter() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
