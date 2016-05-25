using System.Data.Entity;
using NTUST.Models;
using System.Data.Entity.ModelConfiguration.Conventions;


namespace NTUST.DAL
{
    public class SchoolContext : DbContext
    {
        public SchoolContext() : base("SchoolContext") { }

        /* "Student" and "Instructor" inherit from Person */
        public DbSet<Person> People { get; set; }

        public DbSet<Student> Students { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Course> Courses { get; set; }


        public DbSet<Department> Departments { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<OfficeAssignment> OfficeAssignments { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            /* add the new entities to the SchoolContext class and customize some of the mapping 
             * using fluent API calls. The API is "fluent" because it's often used by stringing 
             * a series of method calls together into a single statement, as in the following example:
             */
            modelBuilder.Entity<Course>()
                        .HasMany(c => c.Instructors).WithMany(i => i.Courses)
                        .Map(t => t.MapLeftKey("CourseID")
                        .MapRightKey("InstructorID")
                        .ToTable("CourseInstructor")
                        );

            /* Some developers and DBAs prefer to use stored procedures for database access. In earlier 
             * versions of Entity Framework you can retrieve data using a stored procedure by executing 
             * a raw SQL query, but you can't instruct EF to use stored procedures for update operations. 
             * In EF 6 it's easy to configure Code First to use stored procedures. 
             * 
             * The following code instructs Entity Framework to use stored procedures for insert, update, and 
             * delete operations on the Department entity.
             */
            modelBuilder.Entity<Department>().MapToStoredProcedures();

            /* Add the fluent API for "Optimistic Concurrency" to the Department Entity
             * related code is in Models.Department.cs 
             *      public byte[] RowVersion { get; set; } */
            modelBuilder.Entity<Department>()
                        .Property(p => p.RowVersion).IsConcurrencyToken();


            //2016/5/23 Terrence: SRT=>Add Course StoreProcedures & RowVersion
            modelBuilder.Entity<Course>().MapToStoredProcedures();
            modelBuilder.Entity<Course>()
                        .Property(c => c.RowVersion).IsConcurrencyToken();
            //2016/5/23 Terrence: END

        }
    }
}