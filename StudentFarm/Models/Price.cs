using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;
using UCDArch.Core.DomainModel;
using UCDArch.Core.PersistanceSupport;
using UCDArch.Data.NHibernate;

namespace StudentFarm.Models
{
    public class Price : DomainObjectWithTypedId<int>
    {
        public virtual CropUnit CropUnit { get; set; }
        public virtual double UnitPrice { get; set; }
        public virtual DateTime? PriceDate { get; set; }
        public virtual IList<Offered> Offered { get; set; }

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
            HasMany(x => x.Offered)
                .Cascade.DeleteOrphan()
                .Inverse();
        }
    }

    public interface IPriceRepository : IRepository<Price>
    {
        Price GetOrCreate(CropUnit cu, double price);
    }

    public class PriceRepository : Repository<Price>, IPriceRepository
    {
        public Price GetOrCreate(CropUnit cu, double unitprice)
        {
            // Look for a Price record
            IQueryable<Price> prices = this.Queryable;
            var pq = from price in prices
                     where price.CropUnit.Id == cu.Id &&
                           price.UnitPrice == unitprice
                     select price;

            // Create one if it doesn't exist.
            Price p;
            if (pq.Count() > 0)
            {
                p = pq.First();
            }
            else
            {
                p = new Price();
                p.CropUnit = cu;
                p.UnitPrice = unitprice;
                this.EnsurePersistent(p);
            }

            return p;
        }
    }
}