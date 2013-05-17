using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;
using UCDArch.Core.DomainModel;

namespace StudentFarm.Models
{
    public class Price : DomainObjectWithTypedId<int>
    {
        public virtual CropUnit CropUnit { get; set; }
        public virtual double UnitPrice { get; set; }
        public virtual DateTime? PriceDate { get; set; }

        public Price()
        {
        }
    }

    public class PriceMap : ClassMap<Price>
    {
        public PriceMap()
        {
            Id(x => x.Id).GeneratedBy.Identity();
            References(x => x.CropUnit)
                .Not.Nullable()
                .Cascade.SaveUpdate()
                .Column("CropUnitId");
            Map(x => x.UnitPrice);
            Map(x => x.PriceDate).Nullable();
        }
    }
}