using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using FluentNHibernate.Mapping;
using UCDArch.Core.DomainModel;

namespace StudentFarm.Models
{
    public class User : DomainObjectWithTypedId<int>
    {
        public virtual string Username { get; set; }

        public User()
        {            
        }
    }

    public class UserMap : ClassMap<User>
    {
        public UserMap()
        {
            Id(x => x.Id).GeneratedBy.Identity();
            Map(x => x.Username);
        }
    }
}