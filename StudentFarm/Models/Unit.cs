using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;
using UCDArch.Core.DomainModel;

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
}