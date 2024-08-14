using Chloe.Query;
using Chloe.Reflection;
using System.Data;
using System.Reflection;

namespace Chloe.Mapper.Binders
{
    public class PrimitiveMemberBinder : IMemberBinder
    {
        MapInfo _mapInfo;
        IMRM _mMapper;

        public PrimitiveMemberBinder(MemberInfo member, MRMTuple mrmTuple, int ordinal) : this(new MapInfo() { Member = member, Ordinal = ordinal, MRMTuple = mrmTuple })
        {

        }

        PrimitiveMemberBinder(MapInfo mapInfo)
        {
            this._mapInfo = mapInfo;
        }

        public int Ordinal { get { return this._mapInfo.Ordinal; } }

        public void Prepare(IDataReader reader)
        {
            Type fieldType = reader.GetFieldType(this.Ordinal);
            if (fieldType == this._mapInfo.Member.GetMemberType().GetUnderlyingType())
            {
                this._mMapper = this._mapInfo.MRMTuple.StrongMRM.Value;
                return;
            }

            this._mMapper = this._mapInfo.MRMTuple.SafeMRM.Value;
        }
        public virtual async ValueTask Bind(QueryContext queryContext, object obj, IDataReader reader, bool @async)
        {
            this._mMapper.Map(obj, reader, this._mapInfo.Ordinal);
        }

        public IMemberBinder Clone()
        {
            PrimitiveMemberBinder memberBinder = new PrimitiveMemberBinder(this._mapInfo);
            return memberBinder;
        }

        class MapInfo
        {
            public MemberInfo Member { get; set; }
            public int Ordinal { get; set; }
            public MRMTuple MRMTuple { get; set; }
        }
    }
}
