using Chloe.DbExpressions;
using System.Reflection;

namespace Chloe.RDBMS
{
    /// <summary>
    /// 修正 DbParameterExpression 对象的 DbType
    /// </summary>
    public class DbExpressionNormalizer : DbExpressionVisitor
    {
        static DbExpressionNormalizer Instance = new DbExpressionNormalizer();


        public static DbExpression Normalize(DbExpression exp)
        {
            return exp.Accept(Instance);
        }

        public override DbExpression VisitEqual(DbEqualExpression exp)
        {
            DbExpression left = exp.Left;
            DbExpression right = exp.Right;

            left = PublicHelper.Trim_Nullable_Value(DbExpressionExtension.StripInvalidConvert(left));
            right = PublicHelper.Trim_Nullable_Value(DbExpressionExtension.StripInvalidConvert(right));

            if (IsColumnAccessAndParameter(left, right))
            {
                AmendDbInfo(ref left, ref right);
            }

            return new DbEqualExpression(left, right);
        }

        public override DbExpression VisitNotEqual(DbNotEqualExpression exp)
        {
            DbExpression left = exp.Left;
            DbExpression right = exp.Right;

            left = PublicHelper.Trim_Nullable_Value(DbExpressionExtension.StripInvalidConvert(left));
            right = PublicHelper.Trim_Nullable_Value(DbExpressionExtension.StripInvalidConvert(right));

            if (IsColumnAccessAndParameter(left, right))
            {
                AmendDbInfo(ref left, ref right);
            }

            return new DbNotEqualExpression(left, right);
        }

        public override DbExpression VisitMethodCall(DbMethodCallExpression exp)
        {
            MethodInfo method = exp.Method;

            if (PublicHelper.Is_Sql_IsEqual_Method(method) || PublicHelper.Is_Sql_IsNotEqual_Method(method))
            {
                /*
                 * 修正参数 DbType
                 */

                DbExpression left = exp.Arguments[0];
                DbExpression right = exp.Arguments[1];

                left = PublicHelper.Trim_Nullable_Value(DbExpressionExtension.StripInvalidConvert(left));
                right = PublicHelper.Trim_Nullable_Value(DbExpressionExtension.StripInvalidConvert(right));

                if (IsColumnAccessAndParameter(left, right))
                {
                    AmendDbInfo(ref left, ref right);
                }

                return new DbMethodCallExpression(this.MakeNewExpression(exp.Object), exp.Method, new List<DbExpression>(2) { left.Accept(this), right.Accept(this) });
            }

            if (method.Name == nameof(object.Equals) && method.ReturnType == PublicConstants.TypeOfBoolean && !method.IsStatic && exp.Arguments.Count == 1)
            {
                // obj.Equals(other)

                DbExpression left = exp.Object;
                DbExpression right = exp.Arguments[0];
                if (IsColumnAccessAndParameter(left, right))
                {
                    AmendDbInfo(ref left, ref right);
                }

                return new DbMethodCallExpression(left, exp.Method, new List<DbExpression>(1) { right });
            }

            return base.VisitMethodCall(exp);
        }

        // <
        public override DbExpression VisitLessThan(DbLessThanExpression exp)
        {
            DbExpression left = exp.Left;
            DbExpression right = exp.Right;

            left = PublicHelper.Trim_Nullable_Value(DbExpressionExtension.StripInvalidConvert(left));
            right = PublicHelper.Trim_Nullable_Value(DbExpressionExtension.StripInvalidConvert(right));

            if (IsColumnAccessAndParameter(left, right))
            {
                AmendDbInfo(ref left, ref right);
                return new DbLessThanExpression(left.Accept(this), right.Accept(this), exp.Method);
            }

            return new DbLessThanExpression(exp.Left.Accept(this), exp.Right.Accept(this), exp.Method);
        }

        // <=
        public override DbExpression VisitLessThanOrEqual(DbLessThanOrEqualExpression exp)
        {
            DbExpression left = exp.Left;
            DbExpression right = exp.Right;

            left = PublicHelper.Trim_Nullable_Value(DbExpressionExtension.StripInvalidConvert(left));
            right = PublicHelper.Trim_Nullable_Value(DbExpressionExtension.StripInvalidConvert(right));

            if (IsColumnAccessAndParameter(left, right))
            {
                AmendDbInfo(ref left, ref right);
                return new DbLessThanOrEqualExpression(left.Accept(this), right.Accept(this), exp.Method);
            }

            return new DbLessThanOrEqualExpression(exp.Left.Accept(this), exp.Right.Accept(this), exp.Method);
        }

        // >
        public override DbExpression VisitGreaterThan(DbGreaterThanExpression exp)
        {
            DbExpression left = exp.Left;
            DbExpression right = exp.Right;

            left = PublicHelper.Trim_Nullable_Value(DbExpressionExtension.StripInvalidConvert(left));
            right = PublicHelper.Trim_Nullable_Value(DbExpressionExtension.StripInvalidConvert(right));

            if (IsColumnAccessAndParameter(left, right))
            {
                AmendDbInfo(ref left, ref right);
                return new DbGreaterThanExpression(left.Accept(this), right.Accept(this), exp.Method);
            }

            return new DbGreaterThanExpression(exp.Left.Accept(this), exp.Right.Accept(this), exp.Method);
        }

        // >=
        public override DbExpression VisitGreaterThanOrEqual(DbGreaterThanOrEqualExpression exp)
        {
            DbExpression left = exp.Left;
            DbExpression right = exp.Right;

            left = PublicHelper.Trim_Nullable_Value(DbExpressionExtension.StripInvalidConvert(left));
            right = PublicHelper.Trim_Nullable_Value(DbExpressionExtension.StripInvalidConvert(right));

            if (IsColumnAccessAndParameter(left, right))
            {
                AmendDbInfo(ref left, ref right);
                return new DbGreaterThanOrEqualExpression(left.Accept(this), right.Accept(this), exp.Method);
            }

            return new DbGreaterThanOrEqualExpression(exp.Left.Accept(this), exp.Right.Accept(this), exp.Method);
        }


        public override DbExpression VisitInsert(DbInsertExpression exp)
        {
            DbInsertExpression ret = new DbInsertExpression(exp.Table, exp.InsertColumns.Count, exp.Returns.Count);

            for (int i = 0; i < exp.InsertColumns.Count; i++)
            {
                DbColumn column = exp.InsertColumns[i].Column;
                DbExpression valExp = this.MakeNewExpression(exp.InsertColumns[i].Value);
                AmendDbInfo(column, ref valExp);

                ret.AppendInsertColumn(column, valExp);
            }

            for (int i = 0; i < exp.Returns.Count; i++)
            {
                ret.Returns.Add(exp.Returns[i]);
            }

            return ret;
        }
        public override DbExpression VisitUpdate(DbUpdateExpression exp)
        {
            DbUpdateExpression ret = new DbUpdateExpression(exp.Table, this.MakeNewExpression(exp.Condition), exp.UpdateColumns.Count, exp.Returns.Count);

            for (int i = 0; i < exp.UpdateColumns.Count; i++)
            {
                DbColumn column = exp.UpdateColumns[i].Column;
                DbExpression valExp = this.MakeNewExpression(exp.UpdateColumns[i].Value);
                AmendDbInfo(column, ref valExp);

                ret.AppendUpdateColumn(column, valExp);
            }

            for (int i = 0; i < exp.Returns.Count; i++)
            {
                ret.Returns.Add(exp.Returns[i]);
            }

            return ret;
        }


        static bool IsColumnAccessAndParameter(DbExpression left, DbExpression right)
        {
            return (left.NodeType == DbExpressionType.ColumnAccess && right.NodeType == DbExpressionType.Parameter) || (right.NodeType == DbExpressionType.ColumnAccess && left.NodeType == DbExpressionType.Parameter);
        }

        /// <summary>
        /// 修正参数 DbType
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        static void AmendDbInfo(ref DbExpression left, ref DbExpression right)
        {
            bool hasReversed = false;
            if (right.NodeType == DbExpressionType.ColumnAccess && left.NodeType == DbExpressionType.Parameter)
            {
                var t = right;
                left = right;
                right = t;
                hasReversed = true;
            }

            DbColumnAccessExpression dbColumnAccess = (DbColumnAccessExpression)left;
            DbParameterExpression dbParameter = (DbParameterExpression)right;
            if (dbColumnAccess.Column.DbType != null && dbParameter.DbType == null)
            {
                dbParameter = new DbParameterExpression(dbParameter.Value, dbParameter.Type, dbColumnAccess.Column.DbType);
            }

            if (hasReversed)
            {
                left = dbParameter;
                right = dbColumnAccess;
            }
            else
            {
                left = dbColumnAccess;
                right = dbParameter;
            }
        }

        static void AmendDbInfo(DbColumn column, ref DbExpression exp)
        {
            exp = DbExpressionExtension.StripInvalidConvert(exp);
            if (column.DbType == null || exp.NodeType != DbExpressionType.Parameter)
                return;

            DbParameterExpression dbParameter = (DbParameterExpression)exp;

            if (dbParameter.DbType == null)
            {
                dbParameter = new DbParameterExpression(dbParameter.Value, dbParameter.Type, column.DbType);
                exp = dbParameter;
            }
        }

    }
}
