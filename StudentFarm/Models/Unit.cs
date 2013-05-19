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
    public class Unit : DomainObjectWithTypedId<int>
    {
        public virtual string Name { get; set; }
        public virtual string Notes { get; set; }

        public Unit()
        {
        }
    }

    public class UnitMap : ClassMap<Unit>
    {
        public UnitMap()
        {
            Id(x => x.Id).GeneratedBy.Identity();
            Map(x => x.Name);
            Map(x => x.Notes);
        }
    }

    public interface IUnitRepository : IRepository<Unit>
    {
        Unit GetOrCreate(int id, String name);
    }

    public class UnitRepository : Repository<Unit>, IUnitRepository {
        public Unit GetOrCreate(int id, String name)
        {
            Unit unit = this.GetNullableById(id);

            if (unit == null || unit.Name.ToLower() != name.ToLower())
            {
                unit = new Unit();
                unit.Name = name;
                this.EnsurePersistent(unit);
            }

            return unit;
        }
    }
}