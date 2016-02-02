using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace IntegrationWebSiteMvc.Models
{
    public partial class JobFailure
    {
        #region Auto Generated Code

        /// <remarks/>
        private int mintID;

        /// <remarks/>
        private string mobjSourceSystem;

        /// <remarks/>
        private string mobjType;

        /// <remarks/>
        private System.DateTime mdteDateRecorded;

        /// <remarks/>
        private string mstrExceptionMessage;

        /// <remarks/>
        private int mintRetryCount;

        /// <remarks/>
        private string mstrComments;

        /// <remarks/>
        private string mstrServiceWrapperType;

        /// <summary>
        /// Unique identifier for the entity within the Work Management system
        /// </summary>
        public int ID
        {
            get
            {
                return this.mintID;
            }
            set
            {
                this.mintID = value;
            }
        }

        /// <summary>
        /// Business Stream
        /// </summary>
        public string SourceSystem
        {
            get
            {
                return this.mobjSourceSystem;
            }
            set
            {
                this.mobjSourceSystem = value;
            }
        }

        /// <summary>
        /// Failure Type as defined by the Enumeration, e.g., Import
        /// </summary>
        public string Type
        {
            get
            {
                return this.mobjType;
            }
            set
            {
                this.mobjType = value;
            }
        }

        /// <summary>
        /// The Date the Failure was recorded
        /// </summary>
        public System.DateTime DateRecorded
        {
            get
            {
                return this.mdteDateRecorded;
            }
            set
            {
                this.mdteDateRecorded = value;
            }
        }

        /// <summary>
        /// Exception Message associated with the Failure
        /// </summary>
        public string ExceptionMessage
        {
            get
            {
                return this.mstrExceptionMessage;
            }
            set
            {
                this.mstrExceptionMessage = value;
            }
        }

        /// <summary>
        /// No. of times retried (without success)
        /// </summary>
        public int RetryCount
        {
            get
            {
                return this.mintRetryCount;
            }
            set
            {
                this.mintRetryCount = value;
            }
        }

        /// <summary>
        /// Free text
        /// </summary>
        public string Comments
        {
            get
            {
                return this.mstrComments;
            }
            set
            {
                this.mstrComments = value;
            }
        }

        /// <summary>
        /// ServiceWrapperType within WFBusService (Optional)
        /// </summary>
        public string ServiceWrapperType
        {
            get
            {
                return this.mstrServiceWrapperType;
            }
            set
            {
                this.mstrServiceWrapperType = value;
            }
        }
        #endregion
    }

    public class JobFailureSearchResults : System.Data.Entity.DbContext
    {
        public System.Data.Entity.DbSet<JobFailure> Failures
        {
            get;
            set;
        }

        public JobFailureSearchResults()
            : base("name=TaskStoreConnectionString")
        {
            // Override ConnectionString name to search for in Config file
        }

        public class JobFailureConfiguration : System.Data.Entity.ModelConfiguration.EntityTypeConfiguration<JobFailure>
        {
            public JobFailureConfiguration()
            {
                ToTable("tblFailedJobAudit");

                // The Entity Framework assumes PK column of type "int" that is named ID or "classname" + ID is an Identity. 
                // So you need this nonsense any time a PK included column is not an Identity. Great..
                HasKey(p => p.ID).Property(p => p.ID).HasDatabaseGeneratedOption(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None);

                Property(p => p.ID).HasColumnName("WMSourceID");
                Property(p => p.SourceSystem).HasColumnName("WMSourceSystem");
                Property(p => p.DateRecorded).HasColumnName("FailureDate");
                Property(p => p.Type).HasColumnName("FailureType");
                Ignore(p => p.Comments);
            }
        }

        protected override void OnModelCreating(System.Data.Entity.DbModelBuilder modelBuilder)
        {
            // NB: Pre CTP5 was MapSingleType().ToTable
            // http://blogs.msdn.com/b/adonet/archive/2010/12/10/code-first-mapping-changes-in-ctp5.aspx

            /*
            // Table Name
            modelBuilder.Entity<JobFailure>().ToTable("tblFailedJobAudit");
            // Column Name(s)
            modelBuilder.Entity<JobFailure>()
                .Property(p => p.ID).HasColumnName("WMSourceID");
            */

            /*
            // Table per Hierarchy approach. Requires "Discriminator" column that identifies the (Inherited) Type
             modelBuilder.Entity<Product>()
            .Map(mc => {
                    mc.Requires("disc").HasValue("P");
                })
            .Map<DiscontionuedProduct>(mc => {
                    mc.Requires("disc").HasValue("D");
                });
            */

            modelBuilder.Configurations.Add(new JobFailureConfiguration());

            base.OnModelCreating(modelBuilder);
        }

        public static void Test()
        {
            using (var db = new JobFailureSearchResults())
            {

                // Create and save a new Record

                // Console.Write("Enter a name for a new Record: ");

                // var name = Console.ReadLine();

                var jobFailure = new JobFailure { ID = 1060447, SourceSystem = "Maintenance", DateRecorded = DateTime.Now, Type = "StatusChange" };

                db.Failures.Add(jobFailure);

                db.SaveChanges();

                // Display all Records from the database

                var query = from b in db.Failures

                            orderby b.ID

                            select b;

                Console.WriteLine("All records in the database:");

                foreach (var item in query)
                {

                    Console.WriteLine(item.ID);

                }

                Console.WriteLine("Press any key to exit...");
                // Console.ReadKey();
            }
        }
    }
}