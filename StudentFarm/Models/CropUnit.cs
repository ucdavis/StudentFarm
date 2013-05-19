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

    public interface ICropUnitRepository : IRepository<CropUnit>
    {
        CropUnit GetOrCreate(Crop crop, Unit unit);
    }

    public class CropUnitRepository : Repository<CropUnit>, ICropUnitRepository
    {
        public CropUnit GetOrCreate(Crop crop, Unit unit)
        {
            IQueryable<CropUnit> cropunits = this.Queryable;
            var cuq = from cropunit in cropunits
                      where cropunit.Crop.Id == crop.Id &&
                            cropunit.Unit.Id == unit.Id
                      select cropunit;

            // Create one if it doesn't exist.
            CropUnit cu;
            if (cuq.Count() > 0)
            {
                cu = cuq.First();
            }
            else
            {
                cu = new CropUnit();
                cu.Crop = crop;
                cu.Unit = unit;
                this.EnsurePersistent(cu);
            }

            return cu;
        }
    }
}