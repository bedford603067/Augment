using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IntegrationWebSiteMvc.Models
{
    public class PrimaryArea
    {
        public int ID { get; set; }
        public string Name { get; set; }

        // public virtual ICollection<AreaPostCode> SubAreas { get; set; }
    }

    public class SubArea
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int PrimaryAreaID { get; set; }

        public virtual ICollection<AreaPostCode> PostCodes { get; set; }
    }

    public class AreaPostCode
    {
        public int ID { get; set; }
        public string PostCodePattern { get; set; }
    }

    public class AreaContext : System.Data.Entity.DbContext
    {
        public AreaContext()
            : base("name=PersonnelContext")
        {
            // Override ConnectionString name to search for in Config file
        }

        protected override void OnModelCreating(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AreaPostCode>().HasKey(p => p.ID).Property(p => p.ID).HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<AreaPostCode>().Property(p => p.ID).HasColumnName("AreaPostCodeID");

            modelBuilder.Entity<PrimaryArea>().HasKey(p => p.ID).Property(p => p.ID).HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<PrimaryArea>().Property(p => p.ID).HasColumnName("PrimaryAreaID");
            modelBuilder.Entity<PrimaryArea>().Property(p => p.Name).HasColumnName("AreaName");

            modelBuilder.Entity<SubArea>().HasKey(p => p.ID).Property(p => p.ID).HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<SubArea>().Property(p => p.ID).HasColumnName("SubAreaID");
            modelBuilder.Entity<SubArea>().Property(p => p.Name).HasColumnName("AreaName");

            base.OnModelCreating(modelBuilder);
        }

        public System.Data.Entity.DbSet<PrimaryArea> PrimaryAreas
        {
            get;
            set;
        }

        public System.Data.Entity.DbSet<SubArea> SubAreas
        {
            get;
            set;
        }

        private List<SubArea> GetSubAreas()
        {
            List<SubArea> subAreas = new List<SubArea>();
            System.Data.DataSet dsAreas = null;
            FinalBuild.DataAccess objADO = new FinalBuild.DataAccess(Database.Connection.ConnectionString);
            string storedProcedure = "selAreaInformation";

            dsAreas = objADO.GetDataSet(storedProcedure, null);

            return subAreas;
        }
    }

    public class AreaData
    {
        private List<PrimaryArea> _primaryAreas;
        private List<SubArea> _subAreas;

        public string PrimaryAreaName { get; set; }
        public int SubAreaID { get; set; }
        public string ADLoginID { get; set; }

        public List<PrimaryArea> PrimaryAreas
        {
            get
            {
                return _primaryAreas;
            }
            set
            {
                _primaryAreas = value;
            }
        }

        public List<SubArea> SubAreas
        {
            get
            {
                return _subAreas;
            }
            set
            {
                _subAreas = value;
            }
        }
    }
}