using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.ComponentModel.DataAnnotations;

namespace IntegrationWebSiteMvc.Models
{
    public class Employee
    {
        public Employee()
        {
            // this.AreaDetails = new AreaDetails();
        }

        [Required]
        [StringLength(10, MinimumLength=3)]
        public string ADLoginID { get; set; }
        [Required]
        [StringLength(10, MinimumLength = 2)]
        public string Surname { get; set; }
        
        public string Forenames { get; set; }
        public string EmployeeNo { get; set; }
        public string PreferredName { get; set; }
        
        [RegularExpression(@"^([\w-\.]+@([\w-]+\.)+[\w-]{2,4})$", ErrorMessage = "Please enter a valid email address.")]
        public string EmailAddress { get; set; }

        public string MobileNumber { get; set; }
        public string TelephoneNumber { get; set; }

        public virtual ICollection<BusinessObjects.WorkManagement.Skill> Skills { get; set; }
        public virtual AreaDetails AreaDetails { get; set; }
        public virtual ICollection<PostCodeResponsibility> PostCodeResponsibilities { get; set; }

        public virtual ICollection<SubArea> PostCodeAreas { get; set; }

        public string HomePostCode { get; set; }
        public double? HomeLatitude { get; set; }
        public double? HomeLongitude { get; set; }
        public bool IsFieldUser { get; set; }

        public string PrimaryAreaName { get; set; }

        // public virtual ICollection<EmployeeArea> Areas { get; set; }
    }
    
    /*
    public class Skill
    {
        public int ID { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
    }
    */

    public class AreaDetails
    {
        public int ID { get; set; }
        public string Primary { get; set; }
        public string Sub { get; set; }

        public string Description
        {
            get
            {
                return this.Primary + " - " + this.Sub;
            }
        }
    }

    public class PostCodeResponsibility
    {
        public int ID { get; set; }
        public string PostCodePattern { get; set; }
    }

    public class PersonnelContext : System.Data.Entity.DbContext
    {
        protected override void OnModelCreating(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>().ToTable("Employees");
            modelBuilder.Entity<Employee>().HasKey(p => p.ADLoginID).Property(p => p.ADLoginID).HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None);
            modelBuilder.Entity<Employee>().Ignore(p => p.AreaDetails);

            modelBuilder.Entity<BusinessObjects.WorkManagement.Skill>().ToTable("EmployeeSkills");
            modelBuilder.Entity<BusinessObjects.WorkManagement.Skill>().HasKey(p => p.ID).Property(p => p.ID).HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None);

            modelBuilder.Entity<BusinessObjects.WorkManagement.Skill>().Property(p => p.ID).HasColumnName("SkillID");
            modelBuilder.Entity<BusinessObjects.WorkManagement.Skill>().Property(p => p.Code).HasColumnName("SkillCode");
            modelBuilder.Entity<BusinessObjects.WorkManagement.Skill>().Property(p => p.Description).HasColumnName("SkillDesc");

            modelBuilder.Entity<BusinessObjects.WorkManagement.Skill>().Ignore(p => p.IsDirty);
            modelBuilder.Entity<BusinessObjects.WorkManagement.Skill>().Ignore(p => p.IsNew);
            modelBuilder.Entity<BusinessObjects.WorkManagement.Skill>().Ignore(p => p.NoOfWorkersRequired);
            modelBuilder.Entity<BusinessObjects.WorkManagement.Skill>().Ignore(p => p.SourceSystems);
            modelBuilder.Entity<BusinessObjects.WorkManagement.Skill>().Ignore(p => p.StandardHours);

            modelBuilder.Entity<PostCodeResponsibility>().HasKey(p => p.ID).Property(p => p.ID).HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity);
            modelBuilder.Entity<PostCodeResponsibility>().Property(p => p.ID).HasColumnName("ResponsibilityID");

            modelBuilder.Entity<Employee>().HasMany<BusinessObjects.WorkManagement.Skill>(p => p.Skills);
            modelBuilder.Entity<Employee>().HasMany<PostCodeResponsibility>(p => p.PostCodeResponsibilities);

            // AreaDetails is working due to following a Convention. Attempted to define explicitly, varyimng MapKey as per below, but did NOT work....
            // modelBuilder.Entity<Employee>().HasRequired(p => p.AreaDetails).WithMany().Map(p => p.MapKey("AreaDetails_ID")); // AreaDetails_AreaID

            base.OnModelCreating(modelBuilder);
        }

        public System.Data.Entity.DbSet<Employee> Employees
        {
            get; set;
        }

        public bool SynchroniseData(Employee employee)
        {
            /*
            System.Data.Objects.ObjectContext context = ((System.Data.Entity.Infrastructure.IObjectContextAdapter)this).ObjectContext;
            System.Data.SqlClient.SqlParameter paramADLoginID = new System.Data.SqlClient.SqlParameter("@ADLoginID", employee.ADLoginID);

            var results = context.ExecuteStoreCommand("EXEC updSynchroniseEmployeeData @ADLoginID", new System.Data.SqlClient.SqlParameter[] {paramADLoginID});
            */

            return true;
        }

        public bool HasUnsavedChanges()
        {
            return this.ChangeTracker.Entries().Any(e => e.State == System.Data.Entity.EntityState.Added
                                                      || e.State == System.Data.Entity.EntityState.Modified
                                                      || e.State == System.Data.Entity.EntityState.Deleted);
        }

        public List<SubArea> GetEmployeeAreas(string adLoginID)
        {
            List<SubArea> subAreas = new List<SubArea>();
            List<System.Data.SqlClient.SqlParameter> parameters = new List<System.Data.SqlClient.SqlParameter>(); 
            string storedProcedure = "selEmployeeAreas";

            FinalBuild.DataAccess ado = new FinalBuild.DataAccess(System.Configuration.ConfigurationManager.ConnectionStrings["PersonnelContext"].ConnectionString);
            parameters.Add(new System.Data.SqlClient.SqlParameter("@ADLoginID", adLoginID));
            System.Data.DataTable dtResults = ado.GetDataTable(storedProcedure, "EmployeeAreas", parameters.ToArray());

            for (int index = 0; index < dtResults.Rows.Count; index++)
            {
                subAreas.Add(new SubArea {  ID = (int)dtResults.Rows[index]["SubAreaID"], 
                                            PrimaryAreaID = (int)dtResults.Rows[index]["PrimaryAreaID"], 
                                            Name = dtResults.Rows[index]["AreaName"].ToString() 
                            });
            }

            return subAreas;
        }

        public bool SaveEmployeeArea(string adLoginID, int subAreaID)
        {
            List<System.Data.SqlClient.SqlParameter> parameters = new List<System.Data.SqlClient.SqlParameter>();
            string storedProcedure = "insEmployeeArea";
            int rowsAffected = 0;

            FinalBuild.DataAccess ado = new FinalBuild.DataAccess(System.Configuration.ConfigurationManager.ConnectionStrings["PersonnelContext"].ConnectionString);
            parameters.Add(new System.Data.SqlClient.SqlParameter("@ADLoginID", adLoginID));
            parameters.Add(new System.Data.SqlClient.SqlParameter("@SubAreaID", subAreaID));
            rowsAffected = ado.ExecuteSQL(storedProcedure, parameters.ToArray());

            return (rowsAffected > 0);
        }

        public bool RemoveEmployeeArea(string adLoginID, int subAreaID)
        {
            List<System.Data.SqlClient.SqlParameter> parameters = new List<System.Data.SqlClient.SqlParameter>();
            string storedProcedure = "delEmployeeArea";
            int rowsAffected = 0;

            FinalBuild.DataAccess ado = new FinalBuild.DataAccess(System.Configuration.ConfigurationManager.ConnectionStrings["PersonnelContext"].ConnectionString);
            parameters.Add(new System.Data.SqlClient.SqlParameter("@ADLoginID", adLoginID));
            parameters.Add(new System.Data.SqlClient.SqlParameter("@SubAreaID", subAreaID));
            rowsAffected = ado.ExecuteSQL(storedProcedure, parameters.ToArray());

            return (rowsAffected > 0);
        }
    }
}