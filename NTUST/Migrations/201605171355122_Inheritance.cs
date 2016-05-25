namespace NTUST.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Inheritance : DbMigration
    {
        public override void Up()
        {
            /*
             * ==========================================================
             *                      udpated code
             * ==========================================================
             * This code takes care of the following database update tasks:

                1. Removes foreign key constraints and indexes that point to the Student table.
                2. Renames the Instructor table as Person and makes changes needed for it to store Student data:
                    a). Adds nullable EnrollmentDate for students.
                    b). Adds Discriminator column to indicate whether a row is for a student or an instructor.
                    c). Makes HireDate nullable since student rows won't have hire dates.
                    d). Adds a temporary field that will be used to update foreign keys that point to students. When you copy students into the Person table they'll get new primary key values.
                3. Copies data from the Student table into the Person table. This causes students to get assigned new primary key values.
                4. Fixes foreign key values that point to students.
                5. Re-creates foreign key constraints and indexes, now pointing them to the Person table.
                (If you had used GUID instead of integer as the primary key type, the student primary key values wouldn't have to change, and several of these steps could have been omitted.)

                Run the update-database command again.

             * (In a production system you would make corresponding changes to the Down method in case you 
             * ever had to use that to go back to the previous database version. For this tutorial you won't 
             * be using the Down method.) 
             */

            // Drop foreign keys and indexes that point to tables we're going to drop.
            DropForeignKey("dbo.Enrollment", "StudentID", "dbo.Student");
            DropIndex("dbo.Enrollment", new[] { "StudentID" });

            RenameTable(name: "dbo.Instructor", newName: "Person");
            AddColumn("dbo.Person", "EnrollmentDate", c => c.DateTime());
            AddColumn("dbo.Person", "Discriminator", c => c.String(nullable: false, maxLength: 128, defaultValue: "Instructor"));
            AlterColumn("dbo.Person", "HireDate", c => c.DateTime());
            AddColumn("dbo.Person", "OldId", c => c.Int(nullable: true));

            // Copy existing Student data into new Person table.
            Sql("INSERT INTO dbo.Person (LastName, FirstName, HireDate, EnrollmentDate, Discriminator, OldId)" +
                "SELECT LastName, FirstName, null AS HireDate, EnrollmentDate, 'Student' AS Discriminator, ID AS OldId " +
                "FROM dbo.Student");

            // Fix up existing relationships to match new PK's.
            Sql("UPDATE dbo.Enrollment SET StudentId = (SELECT ID FROM dbo.Person WHERE OldId = " +
                "Enrollment.StudentId AND Discriminator = 'Student')");

            // Remove temporary key
            DropColumn("dbo.Person", "OldId");

            DropTable("dbo.Student");

            // Re-create foreign keys and indexes pointing to new table.
            AddForeignKey("dbo.Enrollment", "StudentID", "dbo.Person", "ID", cascadeDelete: true);
            CreateIndex("dbo.Enrollment", "StudentID");


            /*
             * ==========================================================
             *                      original code
             * ==========================================================
                RenameTable(name: "dbo.Instructor", newName: "Person");
                AddColumn("dbo.Person", "EnrollmentDate", c => c.DateTime());
                AddColumn("dbo.Person", "Discriminator", c => c.String(nullable: false, maxLength: 128));
                AlterColumn("dbo.Person", "HireDate", c => c.DateTime());
                DropTable("dbo.Student");
             */
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.Student",
                c => new
                    {
                        ID = c.Int(nullable: false, identity: true),
                        LastName = c.String(nullable: false, maxLength: 50),
                        FirstName = c.String(nullable: false, maxLength: 50),
                        EnrollmentDate = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.ID);
            
            AlterColumn("dbo.Person", "HireDate", c => c.DateTime(nullable: false));
            DropColumn("dbo.Person", "Discriminator");
            DropColumn("dbo.Person", "EnrollmentDate");
            RenameTable(name: "dbo.Person", newName: "Instructor");
        }
    }
}
