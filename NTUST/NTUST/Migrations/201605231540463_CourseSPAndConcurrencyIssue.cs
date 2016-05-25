namespace NTUST.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class CourseSPAndConcurrencyIssue : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Course", "RowVersion", c => c.Binary(nullable: false, fixedLength: true, timestamp: true, storeType: "rowversion"));
            CreateStoredProcedure(
                "dbo.Course_Insert",
                p => new
                    {
                        CourseID = p.Int(),
                        Title = p.String(maxLength: 50),
                        Credits = p.Int(),
                        DepartmentID = p.Int(),
                    },
                body:
                    @"INSERT [dbo].[Course]([CourseID], [Title], [Credits], [DepartmentID])
                      VALUES (@CourseID, @Title, @Credits, @DepartmentID)
                      
                      SELECT t0.[RowVersion]
                      FROM [dbo].[Course] AS t0
                      WHERE @@ROWCOUNT > 0 AND t0.[CourseID] = @CourseID"
            );
            
            CreateStoredProcedure(
                "dbo.Course_Update",
                p => new
                    {
                        CourseID = p.Int(),
                        Title = p.String(maxLength: 50),
                        Credits = p.Int(),
                        DepartmentID = p.Int(),
                        RowVersion_Original = p.Binary(maxLength: 8, fixedLength: true, storeType: "rowversion"),
                    },
                body:
                    @"UPDATE [dbo].[Course]
                      SET [Title] = @Title, [Credits] = @Credits, [DepartmentID] = @DepartmentID
                      WHERE (([CourseID] = @CourseID) AND (([RowVersion] = @RowVersion_Original) OR ([RowVersion] IS NULL AND @RowVersion_Original IS NULL)))
                      
                      SELECT t0.[RowVersion]
                      FROM [dbo].[Course] AS t0
                      WHERE @@ROWCOUNT > 0 AND t0.[CourseID] = @CourseID"
            );
            
            CreateStoredProcedure(
                "dbo.Course_Delete",
                p => new
                    {
                        CourseID = p.Int(),
                        RowVersion_Original = p.Binary(maxLength: 8, fixedLength: true, storeType: "rowversion"),
                    },
                body:
                    @"DELETE [dbo].[Course]
                      WHERE (([CourseID] = @CourseID) AND (([RowVersion] = @RowVersion_Original) OR ([RowVersion] IS NULL AND @RowVersion_Original IS NULL)))"
            );
            
        }
        
        public override void Down()
        {
            DropStoredProcedure("dbo.Course_Delete");
            DropStoredProcedure("dbo.Course_Update");
            DropStoredProcedure("dbo.Course_Insert");
            DropColumn("dbo.Course", "RowVersion");
        }
    }
}
