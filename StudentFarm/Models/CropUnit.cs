using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;
using UCDArch.Core.DomainModel;

namespace StudentFarm.Models
{
    public class CropUnit : DomainObjectWithTypedId<int>
    {
        public virtual Crop Crop { get; set; }
        public virtual Unit Unit { get; set; }

        public CropUnit()
        {
        }
    }

    public class CropUnitMap : ClassMap<CropUnit>
    {
        public CropUnitMap()
        {
            Id(x => x.Id).GeneratedBy.Identity();
            References(x => x.Crop)
                .Not.Nullable()
                .Cascade.SaveUpdate()
                .Column("CropId");
            References(x => x.Unit)
                .Not.Nullable()
                .Cascade.SaveUpdate()
                .Column("UnitId");
        }
    }
}