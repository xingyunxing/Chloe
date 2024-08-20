using Chloe.Visitors;
using Chloe.DbExpressions;
using Chloe.Extensions;
using Chloe.Infrastructure;
using Chloe.Reflection;
using Chloe.Utility;
using System.Linq.Expressions;
using System.Reflection;

namespace Chloe.Query
{
    class SelectorResolver : ExpressionVisitor<IObjectModel>
    {
        QueryContext _queryContext;
        ExpressionVisitorBase _visitor;
        QueryOptions _queryOptions;
        ScopeParameterDictionary _scopeParameters;
        StringSet _scopeTables;
        QueryModel? _queryModel;

        SelectorResolver(QueryContext queryContext, QueryOptions queryOptions, ScopeParameterDictionary scopeParameters, StringSet scopeTables, QueryModel? queryModel)
        {
            this._queryContext = queryContext;
            this._queryOptions = queryOptions;
            this._scopeParameters = scopeParameters;
            this._scopeTables = scopeTables;
            this._queryModel = queryModel;
        }

        public static IObjectModel Resolve(QueryContext queryContext, LambdaExpression selector, QueryOptions queryOptions, ScopeParameterDictionary scopeParameters, StringSet scopeTables, QueryModel? queryModel)
        {
            SelectorResolver resolver = new SelectorResolver(queryContext, queryOptions, scopeParameters, scopeTables, queryModel);
            return resolver.Visit(selector);
        }

        IObjectModel FindModel(ParameterExpression exp)
        {
            IObjectModel model = this._scopeParameters.Get(exp);
            return model;
        }
        DbExpression ResolveExpression(Expression exp)
        {
            return this._visitor.Visit(exp);
        }
        IObjectModel ResolveComplexMember(MemberExpression exp)
        {
            ParameterExpression p;
            if (ExpressionExtension.IsDerivedFromParameter(exp, out p))
            {
                IObjectModel model = this.FindModel(p);
                return model.GetComplexMember(exp);
            }
            else
            {
                throw new Exception();
            }
        }

        public override IObjectModel Visit(Expression exp)
        {
            if (exp == null)
                return default(IObjectModel);
            switch (exp.NodeType)
            {
                case ExpressionType.Lambda:
                    return this.VisitLambda((LambdaExpression)exp);
                case ExpressionType.New:
                    return this.VisitNew((NewExpression)exp);
                case ExpressionType.MemberInit:
                    return this.VisitMemberInit((MemberInitExpression)exp);
                case ExpressionType.MemberAccess:
                    return this.VisitMemberAccess((MemberExpression)exp);
                case ExpressionType.Parameter:
                    return this.VisitParameter((ParameterExpression)exp);
                default:
                    return this.VisistMapTypeSelector(exp);
            }
        }

        protected override IObjectModel VisitLambda(LambdaExpression exp)
        {
            this._visitor = new GeneralExpressionParser(this._queryContext, this._scopeParameters, this._scopeTables, this._queryModel);
            return this.Visit(exp.Body);
        }
        protected override IObjectModel VisitNew(NewExpression exp)
        {
            IObjectModel result = new ComplexObjectModel(this._queryContext, this._queryOptions, exp.Constructor);
            ParameterInfo[] parames = exp.Constructor.GetParameters();
            for (int i = 0; i < parames.Length; i++)
            {
                ParameterInfo pi = parames[i];
                Expression argExp = exp.Arguments[i];
                if (MappingTypeSystem.IsMappingType(pi.ParameterType))
                {
                    DbExpression dbExpression = this.ResolveExpression(argExp);
                    result.AddConstructorParameter(pi, dbExpression);
                }
                else
                {
                    IObjectModel subResult = this.Visit(argExp);
                    result.AddConstructorParameter(pi, (ComplexObjectModel)subResult);
                }
            }

            return result;
        }
        protected override IObjectModel VisitMemberInit(MemberInitExpression exp)
        {
            IObjectModel result = this.Visit(exp.NewExpression);

            foreach (MemberBinding binding in exp.Bindings)
            {
                if (binding.BindingType != MemberBindingType.Assignment)
                {
                    throw new NotSupportedException();
                }

                MemberAssignment memberAssignment = (MemberAssignment)binding;
                MemberInfo member = memberAssignment.Member;
                Type memberType = member.GetMemberType();

                //是数据库映射类型
                if (MappingTypeSystem.IsMappingType(memberType))
                {
                    DbExpression dbExpression = this.ResolveExpression(memberAssignment.Expression);
                    result.AddPrimitiveMember(member, dbExpression);
                }
                else
                {
                    //对于非数据库映射类型，只支持 NewExpression 和 MemberInitExpression
                    IObjectModel subResult = this.Visit(memberAssignment.Expression);
                    result.AddComplexMember(member, (ComplexObjectModel)subResult);
                }
            }

            return result;
        }
        /// <summary>
        /// a => a.Id   a => a.Name   a => a.User
        /// </summary>
        /// <param name="exp"></param>
        /// <returns></returns>
        protected override IObjectModel VisitMemberAccess(MemberExpression exp)
        {
            //create MappingFieldExpression object if exp is map type
            if (MappingTypeSystem.IsMappingType(exp.Type))
            {
                DbExpression dbExp = this.ResolveExpression(exp);
                PrimitiveObjectModel ret = new PrimitiveObjectModel(this._queryOptions, exp.Type, dbExp);
                return ret;
            }

            //如 a.Order a.User 等形式
            return this.ResolveComplexMember(exp);
        }
        protected override IObjectModel VisitParameter(ParameterExpression exp)
        {
            IObjectModel model = this.FindModel(exp);
            return model;
        }

        IObjectModel VisistMapTypeSelector(Expression exp)
        {
            if (!MappingTypeSystem.IsMappingType(exp.Type))
            {
                throw new NotSupportedException(exp.ToString());
            }

            DbExpression dbExp = this.ResolveExpression(exp);
            PrimitiveObjectModel ret = new PrimitiveObjectModel(this._queryOptions, exp.Type, dbExp);
            return ret;
        }
    }
}
