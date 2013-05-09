using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;
using UCDArch.Core.DomainModel;

namespace StudentFarm.Models
{
    public class CropPrice : DomainObjectWithTypedId<int>
    {
        public virtual Crop Crop { get; set; }
        public virtual Unit Unit { get; set; }
        public virtual double Price { get; set; }
        public virtual DateTime PriceDate { get; set; }

        public CropPrice()
        {
        }
    }

    public class CropPriceMap : ClassMap<CropPrice>
    {
        public CropPriceMap()
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
            Map(x => x.Price);
            Map(x => x.PriceDate);
        }
    }
}