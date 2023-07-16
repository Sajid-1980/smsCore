using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace smsCore.Data.Migrations
{
    /// <inheritdoc />
    public partial class initialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActiveRatings",
                columns: table => new
                {
                    RatingMasterID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffID = table.Column<int>(type: "int", nullable: true),
                    campusID = table.Column<int>(type: "int", nullable: false),
                    ClassID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActiveRatings", x => x.RatingMasterID);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AssetStocks",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PurchasedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Price = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssetStocks", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Books",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccessionNo = table.Column<int>(type: "int", nullable: false),
                    ClassficationNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Author = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Publisher = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Edition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YearOfPublishing = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VolumeNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pages = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ShelfNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PresentPosition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsIssuable = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Translator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cornor = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Books", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "BudgetMasters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampusId = table.Column<int>(type: "int", nullable: false),
                    BudgetName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ToDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Narration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetMasters", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Campuses",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampusName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    mailingName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    mobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    emailId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    web = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Campuses", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MaxStrenth = table.Column<int>(type: "int", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: true),
                    ShortCode = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DailyCashes",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<double>(type: "float", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Particular = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserID = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DailyCashes", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeAttendanceType",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AttendanceName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FineInDays = table.Column<double>(type: "float", nullable: false),
                    YearlyAllowed = table.Column<int>(type: "int", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeAttendanceType", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeMobiles",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StfID = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MobileNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeMobiles", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeWaitingLists",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Qualifaction = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Photo = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Phone2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeWaitingLists", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ExamRemarksDetail",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Code = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamRemarksDetail", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Exams",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exams", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "FeeGroups",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FeeGroupName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeGroups", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "FeeTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TypeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: true),
                    AllowDiscount = table.Column<bool>(type: "bit", nullable: true),
                    AllowEdit = table.Column<bool>(type: "bit", nullable: true),
                    IsOptional = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LedgerPostings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampusId = table.Column<int>(type: "int", nullable: false),
                    voucherTypeId = table.Column<int>(type: "int", nullable: false),
                    voucherNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ledgerId = table.Column<int>(type: "int", nullable: false),
                    debit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    credit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    detailsId = table.Column<int>(type: "int", nullable: false),
                    yearId = table.Column<int>(type: "int", nullable: false),
                    invoiceNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    chequeNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    chequeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LedgerPostings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LibraryBookLists",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccessionNo = table.Column<int>(type: "int", nullable: false),
                    ClassficationNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Author = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Publisher = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Edition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YearOfPublishing = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VolumeNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Language = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pages = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Source = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PresentPosition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsIssuable = table.Column<bool>(type: "bit", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Translator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cornor = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryBookLists", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "MenuActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Type = table.Column<string>(name: "_Type", type: "nvarchar(max)", nullable: true),
                    Controller = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DisplayText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuActions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MobileDevices",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubscriptionId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeviceTypeId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastConnected = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileDevices", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Body = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ViewedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Receipients = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SentByUser = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NotificationType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SentOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "PeriodMasters",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PNo = table.Column<int>(type: "int", nullable: false),
                    Time = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    group = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PeriodMasters", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "PublicMenus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DisplayText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Position = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MenuGroup = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ParentId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicMenus", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PublicMobiles",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MobileNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GroupType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VisibleToAllUser = table.Column<bool>(type: "bit", nullable: true),
                    VisibleToAllCampus = table.Column<bool>(type: "bit", nullable: true),
                    UserID = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicMobiles", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "PublicPages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tags = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PageName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PageTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PageContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicPages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PublicPostCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicPostCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PublicSliders",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstLine = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SecondLine = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThirdLine = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ImagePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SliderGroup = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ButtonText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ButtonUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicSliders", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SchoolInfoes",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SchoolName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Logo = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolInfoes", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "SchoolLogo",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LOGO = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolLogo", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ScienceLabItems",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ItemName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScienceLabItems", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Sections",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SectionName = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sections", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "StudentAttendanceTypes",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AttendanceName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fine = table.Column<double>(type: "float", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentAttendanceTypes", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Students",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GuardianName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StdRelation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Religion = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MotherTongue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastIntitution = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Games = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DOB = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PostalAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OfficeTelephone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PermenantAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FatherQualification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FatherProfession = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FatherNatureOfJob = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FatherDepartment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MotherName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MotherQualification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MotherProfession = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MissingDocuments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CNIC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegistrationNo = table.Column<int>(type: "int", nullable: false),
                    Sex = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResidanceTelephone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GeneralRemarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DateForSubmission = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FatherAlive = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LiveWith = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Domicile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdmissionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdmittedClass = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FeeDiscount = table.Column<double>(type: "float", nullable: true),
                    LastEditedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastEditedOn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApplyLateFee = table.Column<int>(type: "int", nullable: true),
                    StudentCNIC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FatherPhoto = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Students", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Subjects",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCoreSubject = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subjects", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "TaskDairies",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Task = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntryBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TaskDairies", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "tbl_AccountGroup",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountGroupName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GroupUnder = table.Column<int>(type: "int", nullable: true),
                    CrDr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Narration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    Nature = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AffectGrossProfit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_AccountGroup", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Company",
                columns: table => new
                {
                    companyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    companyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    mailingName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    mobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    emailId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    web = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    country = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    state = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    currencyId = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    financialYearFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    booksBeginingFrom = table.Column<DateTime>(type: "datetime2", nullable: true),
                    tin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    cst = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pan = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    currentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    logo = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    extra1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    extra2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    extraDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Company", x => x.companyId);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Designation",
                columns: table => new
                {
                    designationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    designationName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    leaveDays = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    advanceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    narration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    extraDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    extra1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    extra2 = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Designation", x => x.designationId);
                });

            migrationBuilder.CreateTable(
                name: "tbl_FinancialYear",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    financialYearId = table.Column<int>(type: "int", nullable: false),
                    fromDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    toDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_FinancialYear", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tbl_PayHead",
                columns: table => new
                {
                    payHeadId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    payHeadName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    type = table.Column<int>(type: "int", nullable: false),
                    narration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_PayHead", x => x.payHeadId);
                });

            migrationBuilder.CreateTable(
                name: "tbl_SalaryPackage",
                columns: table => new
                {
                    salaryPackageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    salaryPackageName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isActive = table.Column<bool>(type: "bit", nullable: false),
                    narration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_SalaryPackage", x => x.salaryPackageId);
                });

            migrationBuilder.CreateTable(
                name: "tbl_VoucherType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VoucherTypeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TypeOfVoucher = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MethodOfVoucherNumbering = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsTaxApplicable = table.Column<bool>(type: "bit", nullable: false),
                    Narration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    MasterId = table.Column<int>(type: "int", nullable: false),
                    Declaration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Heading1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Heading2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Heading3 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Heading4 = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_VoucherType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TimeTable",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentID = table.Column<int>(type: "int", nullable: false),
                    CampusID = table.Column<int>(type: "int", nullable: false),
                    CampusName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClassID = table.Column<int>(type: "int", nullable: false),
                    ClassName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SectionID = table.Column<int>(type: "int", nullable: false),
                    SectionName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StaffID = table.Column<int>(type: "int", nullable: false),
                    EmployeeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SubjectID = table.Column<int>(type: "int", nullable: false),
                    SubjectName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pno = table.Column<int>(type: "int", nullable: false),
                    TimeFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeTo = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeTable", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "UserDefinitions",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FK = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserType = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDefinitions", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "VoteByStudents",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffID = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Vote = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VoteByStudents", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "WebSites",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    WebLink = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WebSites", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeAwards",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffID = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AwardType = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeAwards", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EmployeeAwards_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "FineDeductions",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<double>(type: "float", nullable: false),
                    StaffID = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Particular = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FineDeductions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_FineDeductions_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "IssuedBooksToStaffs",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookID = table.Column<int>(type: "int", nullable: false),
                    StaffID = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BookCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IssuedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssuedBooksToStaffs", x => x.ID);
                    table.ForeignKey(
                        name: "FK_IssuedBooksToStaffs_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "MobileMessages",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FromId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ToId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MessageType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MobileMessages", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MobileMessages_AspNetUsers_FromId",
                        column: x => x.FromId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "TimeConsumptionForResultEntries",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassID = table.Column<int>(type: "int", nullable: false),
                    ExamHeldID = table.Column<int>(type: "int", nullable: false),
                    ClassSubjectID = table.Column<int>(type: "int", nullable: false),
                    CampusID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LMinute = table.Column<int>(type: "int", nullable: false),
                    LHour = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeConsumptionForResultEntries", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TimeConsumptionForResultEntries_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "BudgetDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BudgetMasterId = table.Column<int>(type: "int", nullable: false),
                    LedgerId = table.Column<int>(type: "int", nullable: false),
                    CrDr = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Credit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Debit = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BudgetDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BudgetDetails_BudgetMasters_BudgetMasterId",
                        column: x => x.BudgetMasterId,
                        principalTable: "BudgetMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CampusUsers",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    CampusID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CampusUsers", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CampusUsers_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CampusUsers_Campuses_CampusID",
                        column: x => x.CampusID,
                        principalTable: "Campuses",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Drivers",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DriverName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CampusID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drivers", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Drivers_Campuses_CampusID",
                        column: x => x.CampusID,
                        principalTable: "Campuses",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeeSlipPaymentMethods",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentMethodName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AllowPartialPayment = table.Column<bool>(type: "bit", nullable: false),
                    CampusID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeSlipPaymentMethods", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeeSlipPaymentMethods_Campuses_CampusID",
                        column: x => x.CampusID,
                        principalTable: "Campuses",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Hostel",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HostelName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalRooms = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalStudents = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CampusId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Hostel", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Hostel_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalTable: "Campuses",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Libraries",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LibraryName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Code = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OpeningTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    ClosingTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    CampusId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Libraries", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Libraries_Campuses_CampusId",
                        column: x => x.CampusId,
                        principalTable: "Campuses",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RatingMasters",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CloseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    RatingType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CampusID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RatingMasters", x => x.ID);
                    table.ForeignKey(
                        name: "FK_RatingMasters_Campuses_CampusID",
                        column: x => x.CampusID,
                        principalTable: "Campuses",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SchoolLeaveSchedule",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    holidayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CampusID = table.Column<int>(type: "int", nullable: false),
                    Color = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsHoliday = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SchoolLeaveSchedule", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SchoolLeaveSchedule_Campuses_CampusID",
                        column: x => x.CampusID,
                        principalTable: "Campuses",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SMSApplicationProperty",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PropertyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PropertyValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CampusID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SMSApplicationProperty", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SMSApplicationProperty_Campuses_CampusID",
                        column: x => x.CampusID,
                        principalTable: "Campuses",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AddmissionWaitingLists",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClassID = table.Column<int>(type: "int", nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CampusID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AddmissionWaitingLists", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AddmissionWaitingLists_Campuses_CampusID",
                        column: x => x.CampusID,
                        principalTable: "Campuses",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AddmissionWaitingLists_Classes_ClassID",
                        column: x => x.ClassID,
                        principalTable: "Classes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LibraryListOfBooks",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Publisher = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClassID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryListOfBooks", x => x.ID);
                    table.ForeignKey(
                        name: "FK_LibraryListOfBooks_Classes_ClassID",
                        column: x => x.ClassID,
                        principalTable: "Classes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ListOfBooks",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Publisher = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClassID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ListOfBooks", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ListOfBooks_Classes_ClassID",
                        column: x => x.ClassID,
                        principalTable: "Classes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamHelds",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExamID = table.Column<int>(type: "int", nullable: false),
                    VeiwAble = table.Column<bool>(type: "bit", nullable: false),
                    CampusID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamHelds", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ExamHelds_Campuses_CampusID",
                        column: x => x.CampusID,
                        principalTable: "Campuses",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamHelds_Exams_ExamID",
                        column: x => x.ExamID,
                        principalTable: "Exams",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClassFeeGroups",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FeeGroupID = table.Column<int>(type: "int", nullable: false),
                    ClassID = table.Column<int>(type: "int", nullable: false),
                    CampusID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassFeeGroups", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ClassFeeGroups_Campuses_CampusID",
                        column: x => x.CampusID,
                        principalTable: "Campuses",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassFeeGroups_Classes_ClassID",
                        column: x => x.ClassID,
                        principalTable: "Classes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassFeeGroups_FeeGroups_FeeGroupID",
                        column: x => x.FeeGroupID,
                        principalTable: "FeeGroups",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeeStructures",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FeeTypeId = table.Column<int>(type: "int", nullable: false),
                    FeeGroupId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Revised = table.Column<bool>(type: "bit", nullable: false),
                    EffectiveFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastEditBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastEditedOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Deduction = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CampusID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeStructures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeeStructures_Campuses_CampusID",
                        column: x => x.CampusID,
                        principalTable: "Campuses",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeeStructures_FeeGroups_FeeGroupId",
                        column: x => x.FeeGroupId,
                        principalTable: "FeeGroups",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeeStructures_FeeTypes_FeeTypeId",
                        column: x => x.FeeTypeId,
                        principalTable: "FeeTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LibraryIssuedBooks",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LibraryBookRecordId = table.Column<int>(type: "int", nullable: false),
                    StudentID = table.Column<int>(type: "int", nullable: true),
                    IssuedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BookCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LibraryBookListID = table.Column<int>(type: "int", nullable: true),
                    AspNetUserId = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryIssuedBooks", x => x.ID);
                    table.ForeignKey(
                        name: "FK_LibraryIssuedBooks_AspNetUsers_AspNetUserId",
                        column: x => x.AspNetUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LibraryIssuedBooks_LibraryBookLists_LibraryBookListID",
                        column: x => x.LibraryBookListID,
                        principalTable: "LibraryBookLists",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "MenuItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParentId = table.Column<int>(type: "int", nullable: false),
                    Icon = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    ActionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MenuItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MenuItems_MenuActions_ActionId",
                        column: x => x.ActionId,
                        principalTable: "MenuActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Privlidges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ActionId = table.Column<int>(type: "int", nullable: false),
                    Allow = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Privlidges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Privlidges_MenuActions_ActionId",
                        column: x => x.ActionId,
                        principalTable: "MenuActions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PublicPosts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PreviewImage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DetailImage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ShortDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostContent = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PostCategoryId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsPublished = table.Column<bool>(type: "bit", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PublicPosts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PublicPosts_PublicPostCategories_PostCategoryId",
                        column: x => x.PostCategoryId,
                        principalTable: "PublicPostCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScienceLabItemsStocks",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LIID = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Stock = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Quantity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Price = table.Column<double>(type: "float", nullable: true),
                    Size = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CampusID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScienceLabItemsStocks", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ScienceLabItemsStocks_Campuses_CampusID",
                        column: x => x.CampusID,
                        principalTable: "Campuses",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScienceLabItemsStocks_ScienceLabItems_LIID",
                        column: x => x.LIID,
                        principalTable: "ScienceLabItems",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScienceLabMissingItems",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LIID = table.Column<int>(type: "int", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Demand = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Size = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CampusID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScienceLabMissingItems", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ScienceLabMissingItems_Campuses_CampusID",
                        column: x => x.CampusID,
                        principalTable: "Campuses",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScienceLabMissingItems_ScienceLabItems_LIID",
                        column: x => x.LIID,
                        principalTable: "ScienceLabItems",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ClassSections",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SectionID = table.Column<int>(type: "int", nullable: false),
                    ClassID = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CampusID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassSections", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ClassSections_Campuses_CampusID",
                        column: x => x.CampusID,
                        principalTable: "Campuses",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassSections_Classes_ClassID",
                        column: x => x.ClassID,
                        principalTable: "Classes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassSections_Sections_SectionID",
                        column: x => x.SectionID,
                        principalTable: "Sections",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComSubs",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Attendance = table.Column<int>(type: "int", nullable: false),
                    Result = table.Column<int>(type: "int", nullable: false),
                    IssuedFee = table.Column<int>(type: "int", nullable: false),
                    ReceiveFee = table.Column<int>(type: "int", nullable: false),
                    StudentID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComSubs", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ComSubs_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DOBChanges",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OldDOB = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NewDOB = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    EditDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReferenceNo = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DOBChanges", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DOBChanges_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DOBChanges_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExpellDetails",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Particular = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LastClass = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SLCNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Conduct = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ExpellType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Dues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastCampus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastSection = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpellDetails", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ExpellDetails_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExpellDetails_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeeDiscounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    FeeTypeId = table.Column<int>(type: "int", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountInAmount = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EditedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeDiscounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeeDiscounts_FeeTypes_FeeTypeId",
                        column: x => x.FeeTypeId,
                        principalTable: "FeeTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeeDiscounts_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FreeAdmissions",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FreeAdmissions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_FreeAdmissions_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IssuedBoardCertificates",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Level = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Certificate = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RollNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CertificateNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IssuedTo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IssuedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StdID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssuedBoardCertificates", x => x.ID);
                    table.ForeignKey(
                        name: "FK_IssuedBoardCertificates_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IssuedBoardCertificates_Students_StdID",
                        column: x => x.StdID,
                        principalTable: "Students",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "IssuedBooks",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookID = table.Column<int>(type: "int", nullable: false),
                    StudentID = table.Column<int>(type: "int", nullable: true),
                    IssuedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BookCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StaffID = table.Column<decimal>(type: "decimal(18,2)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssuedBooks", x => x.ID);
                    table.ForeignKey(
                        name: "FK_IssuedBooks_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IssuedBooks_Books_BookID",
                        column: x => x.BookID,
                        principalTable: "Books",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IssuedBooks_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "IssuedCertificates",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CertificateType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SNo = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IssuedCertificates", x => x.ID);
                    table.ForeignKey(
                        name: "FK_IssuedCertificates_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_IssuedCertificates_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MessageRecords",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TextMessage = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    PerSMSCharge = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MessageRecords", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MessageRecords_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MessageRecords_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OldJinnahians",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Studied = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JobTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JoiningDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OfficeAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomeAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultCellNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CellNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OfficePhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResidancePhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Facebook = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Photo = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    StudentID = table.Column<int>(type: "int", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    remarks = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OldJinnahians", x => x.ID);
                    table.ForeignKey(
                        name: "FK_OldJinnahians_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "ParentsVisits",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WhoVisited = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Particular = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StudentID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParentsVisits", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ParentsVisits_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProhibitedMaterials",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Material = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StudentID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProhibitedMaterials", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ProhibitedMaterials_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SelectedForCadetColleges",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Year = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SelectedForCadetColleges", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SelectedForCadetColleges_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_SelectedForCadetColleges_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SignatureSpeciman",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FatherSign = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    GaurdianSign = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    StudentID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SignatureSpeciman", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SignatureSpeciman_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentAlumnis",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Studied = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JobTitle = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Department = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    JoiningDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OfficeAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HomeAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DefaultCellNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CellNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OfficePhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResidancePhone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Facebook = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Photo = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    StudentID = table.Column<int>(type: "int", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Campus = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentAlumnis", x => x.ID);
                    table.ForeignKey(
                        name: "FK_StudentAlumnis_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "StudentComplaigns",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Particular = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Ctype = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmployeeId = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentComplaigns", x => x.ID);
                    table.ForeignKey(
                        name: "FK_StudentComplaigns_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentMobiles",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    MobileHolder = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MobileNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Relation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StudentID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentMobiles", x => x.ID);
                    table.ForeignKey(
                        name: "FK_StudentMobiles_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentPhotos",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    StudentImage = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    PhotoYear = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsReplaced = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentPhotos", x => x.ID);
                    table.ForeignKey(
                        name: "FK_StudentPhotos_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentsWarnings",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WarningNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Reson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentsWarnings", x => x.ID);
                    table.ForeignKey(
                        name: "FK_StudentsWarnings_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentThumbImpressions",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    Finger = table.Column<int>(type: "int", nullable: false),
                    Thumb = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentThumbImpressions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_StudentThumbImpressions_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UnwantedClients",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StdID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CNIC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StudentID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnwantedClients", x => x.ID);
                    table.ForeignKey(
                        name: "FK_UnwantedClients_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UnwantedClients_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "ClassSubjects",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectID = table.Column<int>(type: "int", nullable: false),
                    ClassID = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: true),
                    CampusID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassSubjects", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ClassSubjects_Campuses_CampusID",
                        column: x => x.CampusID,
                        principalTable: "Campuses",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassSubjects_Classes_ClassID",
                        column: x => x.ClassID,
                        principalTable: "Classes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassSubjects_Subjects_SubjectID",
                        column: x => x.SubjectID,
                        principalTable: "Subjects",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_AccountLedger",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AccountGroupId = table.Column<int>(type: "int", nullable: false),
                    LedgerName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OpeningBalance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsDefault = table.Column<bool>(type: "bit", nullable: false),
                    CrOrDr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Narration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MailingName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Mobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreditPeriod = table.Column<int>(type: "int", nullable: false),
                    CreditLimit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    AccountNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BranchName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BranchCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_AccountLedger", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_AccountLedger_tbl_AccountGroup_AccountGroupId",
                        column: x => x.AccountGroupId,
                        principalTable: "tbl_AccountGroup",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_Employee",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    designationId = table.Column<int>(type: "int", nullable: false),
                    employeeName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    employeeCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dob = table.Column<DateTime>(type: "datetime2", nullable: true),
                    maritalStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    gender = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    qualification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    phoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    mobileNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    joiningDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    terminationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    isActive = table.Column<bool>(type: "bit", nullable: true),
                    narration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    bloodGroup = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    passportNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    passportExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    labourCardNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    labourCardExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    visaNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    visaExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    salaryType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    dailyWage = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    bankName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    branchName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    bankAccountNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    branchCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    panNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    pfNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    esiNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    extraDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    extra1 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    extra2 = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    defaultPackageId = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Photo = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    FatherName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CNIC = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CampusID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_Employee", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_Employee_Campuses_CampusID",
                        column: x => x.CampusID,
                        principalTable: "Campuses",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_Employee_tbl_Designation_designationId",
                        column: x => x.designationId,
                        principalTable: "tbl_Designation",
                        principalColumn: "designationId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_SalaryPackageDetails",
                columns: table => new
                {
                    salaryPackageDetailsId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    salaryPackageId = table.Column<int>(type: "int", nullable: false),
                    payHeadId = table.Column<int>(type: "int", nullable: false),
                    amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    narration = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_SalaryPackageDetails", x => x.salaryPackageDetailsId);
                    table.ForeignKey(
                        name: "FK_tbl_SalaryPackageDetails_tbl_PayHead_payHeadId",
                        column: x => x.payHeadId,
                        principalTable: "tbl_PayHead",
                        principalColumn: "payHeadId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_SalaryPackageDetails_tbl_SalaryPackage_salaryPackageId",
                        column: x => x.salaryPackageId,
                        principalTable: "tbl_SalaryPackage",
                        principalColumn: "salaryPackageId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentsTransports",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    DriverID = table.Column<int>(type: "int", nullable: false),
                    PickPoint = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PickTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TripNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Fare = table.Column<double>(type: "float", nullable: false),
                    Started = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Closed = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentsTransports", x => x.ID);
                    table.ForeignKey(
                        name: "FK_StudentsTransports_Drivers_DriverID",
                        column: x => x.DriverID,
                        principalTable: "Drivers",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentsTransports_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Floor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Capacity = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RoomNo = table.Column<int>(type: "int", nullable: false),
                    HostelId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Rooms_Hostel_HostelId",
                        column: x => x.HostelId,
                        principalTable: "Hostel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LibraryBookRecords",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LibraryId = table.Column<int>(type: "int", nullable: false),
                    LibraryBookListId = table.Column<int>(type: "int", nullable: false),
                    ShelfNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    isDeleted = table.Column<bool>(type: "bit", nullable: false),
                    isActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryBookRecords", x => x.ID);
                    table.ForeignKey(
                        name: "FK_LibraryBookRecords_Libraries_LibraryId",
                        column: x => x.LibraryId,
                        principalTable: "Libraries",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LibraryBookRecords_LibraryBookLists_LibraryBookListId",
                        column: x => x.LibraryBookListId,
                        principalTable: "LibraryBookLists",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ratings",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MasterID = table.Column<int>(type: "int", nullable: false),
                    Star = table.Column<int>(type: "int", nullable: true),
                    Star1 = table.Column<int>(type: "int", nullable: true),
                    Star2 = table.Column<int>(type: "int", nullable: true),
                    Star3 = table.Column<int>(type: "int", nullable: true),
                    Star4 = table.Column<int>(type: "int", nullable: true),
                    Star5 = table.Column<int>(type: "int", nullable: true),
                    StaffID = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Total = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ratings", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Ratings_RatingMasters_MasterID",
                        column: x => x.MasterID,
                        principalTable: "RatingMasters",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamsRules",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    APlus = table.Column<double>(type: "float", nullable: false),
                    B = table.Column<double>(type: "float", nullable: false),
                    C = table.Column<double>(type: "float", nullable: false),
                    D = table.Column<double>(type: "float", nullable: false),
                    E = table.Column<double>(type: "float", nullable: false),
                    CoreFail = table.Column<int>(type: "int", nullable: false),
                    NoneCoreFail = table.Column<int>(type: "int", nullable: false),
                    CoreWithNoneCoreFail = table.Column<int>(type: "int", nullable: false),
                    AtLeastPercentage = table.Column<double>(type: "float", nullable: false),
                    A = table.Column<double>(type: "float", nullable: false),
                    BPlus = table.Column<double>(type: "float", nullable: false),
                    ExamHeldID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamsRules", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ExamsRules_ExamHelds_ExamHeldID",
                        column: x => x.ExamHeldID,
                        principalTable: "ExamHelds",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ResultSummaries",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamHeldID = table.Column<int>(type: "int", nullable: false),
                    CampusName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClassName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SectionName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Enrollment = table.Column<int>(type: "int", nullable: true),
                    Success = table.Column<int>(type: "int", nullable: true),
                    Fail = table.Column<int>(type: "int", nullable: true),
                    Percentage = table.Column<double>(type: "float", nullable: true),
                    S1 = table.Column<int>(type: "int", nullable: true),
                    S2 = table.Column<int>(type: "int", nullable: true),
                    S3 = table.Column<int>(type: "int", nullable: true),
                    C1 = table.Column<int>(type: "int", nullable: true),
                    C2 = table.Column<int>(type: "int", nullable: true),
                    C3 = table.Column<int>(type: "int", nullable: true),
                    SB1 = table.Column<int>(type: "int", nullable: true),
                    SB2 = table.Column<int>(type: "int", nullable: true),
                    SB3 = table.Column<int>(type: "int", nullable: true),
                    Appeared = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ResultSummaries", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ResultSummaries_ExamHelds_ExamHeldID",
                        column: x => x.ExamHeldID,
                        principalTable: "ExamHelds",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Admissions",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsExpell = table.Column<bool>(type: "bit", nullable: false),
                    ClassSectionID = table.Column<int>(type: "int", nullable: false),
                    StudentID = table.Column<int>(type: "int", nullable: false),
                    Session = table.Column<int>(type: "int", nullable: false),
                    CampuseID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admissions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Admissions_Campuses_CampuseID",
                        column: x => x.CampuseID,
                        principalTable: "Campuses",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Admissions_ClassSections_ClassSectionID",
                        column: x => x.ClassSectionID,
                        principalTable: "ClassSections",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Admissions_Students_StudentID",
                        column: x => x.StudentID,
                        principalTable: "Students",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamDates",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExamDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExamHeldID = table.Column<int>(type: "int", nullable: false),
                    TimeFrom = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ToTime = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalMarks = table.Column<double>(type: "float", nullable: false),
                    ClassSectionID = table.Column<int>(type: "int", nullable: false),
                    CampusID = table.Column<int>(type: "int", nullable: false),
                    SubjectID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamDates", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ExamDates_Campuses_CampusID",
                        column: x => x.CampusID,
                        principalTable: "Campuses",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamDates_ClassSections_ClassSectionID",
                        column: x => x.ClassSectionID,
                        principalTable: "ClassSections",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_ExamDates_ExamHelds_ExamHeldID",
                        column: x => x.ExamHeldID,
                        principalTable: "ExamHelds",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_ExamDates_Subjects_SubjectID",
                        column: x => x.SubjectID,
                        principalTable: "Subjects",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Homework",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    HomeworkDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmissionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AttachDocument = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClassSectionId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Homework", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Homework_ClassSections_ClassSectionId",
                        column: x => x.ClassSectionId,
                        principalTable: "ClassSections",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Homework_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TeachingClasses",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassSectionId = table.Column<int>(type: "int", nullable: false),
                    StaffID = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingClasses", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TeachingClasses_ClassSections_ClassSectionId",
                        column: x => x.ClassSectionId,
                        principalTable: "ClassSections",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SLCs",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpellDetailID = table.Column<int>(type: "int", nullable: true),
                    UserID = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SLCs", x => x.ID);
                    table.ForeignKey(
                        name: "FK_SLCs_ExpellDetails_ExpellDetailID",
                        column: x => x.ExpellDetailID,
                        principalTable: "ExpellDetails",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "ExpenseMasters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampusId = table.Column<int>(type: "int", nullable: false),
                    voucherNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    invoiceNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LedgerId = table.Column<int>(type: "int", nullable: false),
                    Narration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VoucherTypeId = table.Column<int>(type: "int", nullable: false),
                    ChequeNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChequeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VocherDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseMasters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpenseMasters_tbl_AccountLedger_LedgerId",
                        column: x => x.LedgerId,
                        principalTable: "tbl_AccountLedger",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExpenseMasters_tbl_VoucherType_VoucherTypeId",
                        column: x => x.VoucherTypeId,
                        principalTable: "tbl_VoucherType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "JournalMasters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampusId = table.Column<int>(type: "int", nullable: false),
                    voucherNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    invoiceNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LedgerId = table.Column<int>(type: "int", nullable: false),
                    Narration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VoucherTypeId = table.Column<int>(type: "int", nullable: false),
                    ChequeNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChequeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VocherDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalMasters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalMasters_tbl_AccountLedger_LedgerId",
                        column: x => x.LedgerId,
                        principalTable: "tbl_AccountLedger",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JournalMasters_tbl_VoucherType_VoucherTypeId",
                        column: x => x.VoucherTypeId,
                        principalTable: "tbl_VoucherType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PaymentMasters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampusId = table.Column<int>(type: "int", nullable: false),
                    voucherNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    invoiceNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LedgerId = table.Column<int>(type: "int", nullable: false),
                    Narration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VoucherTypeId = table.Column<int>(type: "int", nullable: false),
                    ChequeNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChequeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VocherDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentMasters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentMasters_tbl_AccountLedger_LedgerId",
                        column: x => x.LedgerId,
                        principalTable: "tbl_AccountLedger",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentMasters_tbl_VoucherType_VoucherTypeId",
                        column: x => x.VoucherTypeId,
                        principalTable: "tbl_VoucherType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ReceiptMasters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampusId = table.Column<int>(type: "int", nullable: false),
                    voucherNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    invoiceNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LedgerId = table.Column<int>(type: "int", nullable: false),
                    Narration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VoucherTypeId = table.Column<int>(type: "int", nullable: false),
                    ChequeNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChequeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VocherDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptMasters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceiptMasters_tbl_AccountLedger_LedgerId",
                        column: x => x.LedgerId,
                        principalTable: "tbl_AccountLedger",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReceiptMasters_tbl_VoucherType_VoucherTypeId",
                        column: x => x.VoucherTypeId,
                        principalTable: "tbl_VoucherType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationLeave",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: true),
                    DateFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateTo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationLeave", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationLeave_tbl_Employee_StaffId",
                        column: x => x.StaffId,
                        principalTable: "tbl_Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeAttendance",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AttendanceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StaffID = table.Column<int>(type: "int", nullable: false),
                    AttendanceTypeID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeAttendance", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EmployeeAttendance_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmployeeAttendance_EmployeeAttendanceType_AttendanceTypeID",
                        column: x => x.AttendanceTypeID,
                        principalTable: "EmployeeAttendanceType",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeAttendance_tbl_Employee_StaffID",
                        column: x => x.StaffID,
                        principalTable: "tbl_Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeExperinceSubjects",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeExperinceSubjects", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EmployeeExperinceSubjects_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeExperinceSubjects_tbl_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "tbl_Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeExprience",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StfID = table.Column<int>(type: "int", nullable: false),
                    Institution = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Desgnition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    From = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    To = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeExprience", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EmployeeExprience_tbl_Employee_StfID",
                        column: x => x.StfID,
                        principalTable: "tbl_Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeShortLeave",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffID = table.Column<int>(type: "int", nullable: false),
                    OutTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeShortLeave", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EmployeeShortLeave_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmployeeShortLeave_tbl_Employee_StaffID",
                        column: x => x.StaffID,
                        principalTable: "tbl_Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeThumbImpressions",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Thumb = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Finger = table.Column<int>(type: "int", nullable: false),
                    StaffID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeThumbImpressions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_EmployeeThumbImpressions_tbl_Employee_StaffID",
                        column: x => x.StaffID,
                        principalTable: "tbl_Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LeavedStaffs",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    LeavingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeavedStaffs", x => x.ID);
                    table.ForeignKey(
                        name: "FK_LeavedStaffs_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LeavedStaffs_tbl_Employee_StaffID",
                        column: x => x.StaffID,
                        principalTable: "tbl_Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProfessionalTrainings",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    University = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Qualification = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Year = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Grade = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfessionalTrainings", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ProfessionalTrainings_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ProfessionalTrainings_tbl_Employee_StaffID",
                        column: x => x.StaffID,
                        principalTable: "tbl_Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Qualifications",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StaffID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    University = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QualificationType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Year = table.Column<int>(type: "int", nullable: false),
                    Grade = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Qualifications", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Qualifications_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Qualifications_tbl_Employee_StaffID",
                        column: x => x.StaffID,
                        principalTable: "tbl_Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_MonthlySalary",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ForMonth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    SalaryPackageId = table.Column<int>(type: "int", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_MonthlySalary", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_MonthlySalary_tbl_Employee_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "tbl_Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_MonthlySalary_tbl_SalaryPackage_SalaryPackageId",
                        column: x => x.SalaryPackageId,
                        principalTable: "tbl_SalaryPackage",
                        principalColumn: "salaryPackageId");
                });

            migrationBuilder.CreateTable(
                name: "TeachingSubject",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampusID = table.Column<int>(type: "int", nullable: false),
                    ClassSectionID = table.Column<int>(type: "int", nullable: false),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    StaffID = table.Column<int>(type: "int", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CloseDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TeachingSubject", x => x.ID);
                    table.ForeignKey(
                        name: "FK_TeachingSubject_Campuses_CampusID",
                        column: x => x.CampusID,
                        principalTable: "Campuses",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeachingSubject_ClassSections_ClassSectionID",
                        column: x => x.ClassSectionID,
                        principalTable: "ClassSections",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_TeachingSubject_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TeachingSubject_tbl_Employee_StaffID",
                        column: x => x.StaffID,
                        principalTable: "tbl_Employee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "LibraryIssuedBooksToStaffs",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LibraryBookRecordId = table.Column<int>(type: "int", nullable: false),
                    StaffID = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IssuedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ReturnDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BookCondition = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LibraryIssuedBooksToStaffs", x => x.ID);
                    table.ForeignKey(
                        name: "FK_LibraryIssuedBooksToStaffs_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LibraryIssuedBooksToStaffs_LibraryBookRecords_LibraryBookRecordId",
                        column: x => x.LibraryBookRecordId,
                        principalTable: "LibraryBookRecords",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CallRecords",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdmissionID = table.Column<int>(type: "int", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Durtion = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CallRecords", x => x.ID);
                    table.ForeignKey(
                        name: "FK_CallRecords_Admissions_AdmissionID",
                        column: x => x.AdmissionID,
                        principalTable: "Admissions",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExamRemarks",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdmissionID = table.Column<int>(type: "int", nullable: false),
                    ExamHeldID = table.Column<int>(type: "int", nullable: false),
                    SocBeh = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Discpline = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Manners = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OralExp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CoTeamWork = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Confid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Punctuality = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhyFit = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Neatness = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Attendance = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExamRemarks", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ExamRemarks_Admissions_AdmissionID",
                        column: x => x.AdmissionID,
                        principalTable: "Admissions",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExamRemarks_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ExamRemarks_ExamHelds_ExamHeldID",
                        column: x => x.ExamHeldID,
                        principalTable: "ExamHelds",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "FeeSlips",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdmissionId = table.Column<int>(type: "int", nullable: false),
                    ForMonth = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EditeOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastFineDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeSlips", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeeSlips_Admissions_AdmissionId",
                        column: x => x.AdmissionId,
                        principalTable: "Admissions",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HostelAdmissions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoomId = table.Column<int>(type: "int", nullable: false),
                    HostelId = table.Column<int>(type: "int", nullable: false),
                    AdmisionId = table.Column<int>(type: "int", nullable: false),
                    AdmissionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Fee = table.Column<int>(type: "int", nullable: false),
                    Isexpel = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HostelAdmissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HostelAdmissions_Admissions_AdmisionId",
                        column: x => x.AdmisionId,
                        principalTable: "Admissions",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HostelAdmissions_Hostel_HostelId",
                        column: x => x.HostelId,
                        principalTable: "Hostel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_HostelAdmissions_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "RatingParticipants",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MasterID = table.Column<int>(type: "int", nullable: false),
                    AdmissionID = table.Column<int>(type: "int", nullable: false),
                    StaffID = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Rate = table.Column<int>(type: "int", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RatingParticipants", x => x.ID);
                    table.ForeignKey(
                        name: "FK_RatingParticipants_Admissions_AdmissionID",
                        column: x => x.AdmissionID,
                        principalTable: "Admissions",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RatingParticipants_RatingMasters_MasterID",
                        column: x => x.MasterID,
                        principalTable: "RatingMasters",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "ReAdmissions",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdmissionID = table.Column<int>(type: "int", nullable: false),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Particular = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserID = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReAdmissions", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ReAdmissions_Admissions_AdmissionID",
                        column: x => x.AdmissionID,
                        principalTable: "Admissions",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Results",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClassSubjectID = table.Column<int>(type: "int", nullable: false),
                    ExamHeldID = table.Column<int>(type: "int", nullable: false),
                    ObtainedMarks = table.Column<double>(type: "float", nullable: false),
                    AdmissionID = table.Column<int>(type: "int", nullable: false),
                    CheckedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Results", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Results_Admissions_AdmissionID",
                        column: x => x.AdmissionID,
                        principalTable: "Admissions",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Results_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Results_ClassSubjects_ClassSubjectID",
                        column: x => x.ClassSubjectID,
                        principalTable: "ClassSubjects",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Results_ExamHelds_ExamHeldID",
                        column: x => x.ExamHeldID,
                        principalTable: "ExamHelds",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "StudentAttendences",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AttendanceDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdmissionID = table.Column<int>(type: "int", nullable: false),
                    FineAmount = table.Column<double>(type: "float", nullable: false),
                    AttendanceTypeID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    sendmessage = table.Column<bool>(type: "bit", nullable: true),
                    manaual = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentAttendences", x => x.ID);
                    table.ForeignKey(
                        name: "FK_StudentAttendences_Admissions_AdmissionID",
                        column: x => x.AdmissionID,
                        principalTable: "Admissions",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_StudentAttendences_AspNetUsers_UserID",
                        column: x => x.UserID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_StudentAttendences_StudentAttendanceTypes_AttendanceTypeID",
                        column: x => x.AttendanceTypeID,
                        principalTable: "StudentAttendanceTypes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StudentPreviousEducations",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdmissionID = table.Column<int>(type: "int", nullable: false),
                    PreviousBoard = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    YearOfPassing = table.Column<int>(type: "int", nullable: false),
                    ObtainedMarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PreviousGroup = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RegNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Grade = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AdmissionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Session = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RollNo = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentPreviousEducations", x => x.ID);
                    table.ForeignKey(
                        name: "FK_StudentPreviousEducations_Admissions_AdmissionID",
                        column: x => x.AdmissionID,
                        principalTable: "Admissions",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExpenseDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExpenseMasterId = table.Column<int>(type: "int", nullable: false),
                    LedgerId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExpenseDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExpenseDetails_ExpenseMasters_ExpenseMasterId",
                        column: x => x.ExpenseMasterId,
                        principalTable: "ExpenseMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExpenseDetails_tbl_AccountLedger_LedgerId",
                        column: x => x.LedgerId,
                        principalTable: "tbl_AccountLedger",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "JournalDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JournalMasterId = table.Column<int>(type: "int", nullable: false),
                    LedgerId = table.Column<int>(type: "int", nullable: false),
                    Debit = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Credit = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JournalDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JournalDetails_JournalMasters_JournalMasterId",
                        column: x => x.JournalMasterId,
                        principalTable: "JournalMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JournalDetails_tbl_AccountLedger_LedgerId",
                        column: x => x.LedgerId,
                        principalTable: "tbl_AccountLedger",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "PaymentDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PaymentMasterId = table.Column<int>(type: "int", nullable: false),
                    LedgerId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentDetails_PaymentMasters_PaymentMasterId",
                        column: x => x.PaymentMasterId,
                        principalTable: "PaymentMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentDetails_tbl_AccountLedger_LedgerId",
                        column: x => x.LedgerId,
                        principalTable: "tbl_AccountLedger",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "ReceiptDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReceiptMasterId = table.Column<int>(type: "int", nullable: false),
                    LedgerId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReceiptDetails_ReceiptMasters_ReceiptMasterId",
                        column: x => x.ReceiptMasterId,
                        principalTable: "ReceiptMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReceiptDetails_tbl_AccountLedger_LedgerId",
                        column: x => x.LedgerId,
                        principalTable: "tbl_AccountLedger",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "tbl_MonthlySalaryDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MonthlySalaryId = table.Column<int>(type: "int", nullable: false),
                    PayHeadId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    type = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_MonthlySalaryDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_MonthlySalaryDetails_tbl_MonthlySalary_MonthlySalaryId",
                        column: x => x.MonthlySalaryId,
                        principalTable: "tbl_MonthlySalary",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_MonthlySalaryDetails_tbl_PayHead_PayHeadId",
                        column: x => x.PayHeadId,
                        principalTable: "tbl_PayHead",
                        principalColumn: "payHeadId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tbl_SalaryPayment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MonthlySalaryId = table.Column<int>(type: "int", nullable: false),
                    LedgerId = table.Column<int>(type: "int", nullable: false),
                    VoucherNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Chequenumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ChequeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Narration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VoucherTypeId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tbl_SalaryPayment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tbl_SalaryPayment_tbl_AccountLedger_LedgerId",
                        column: x => x.LedgerId,
                        principalTable: "tbl_AccountLedger",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_SalaryPayment_tbl_MonthlySalary_MonthlySalaryId",
                        column: x => x.MonthlySalaryId,
                        principalTable: "tbl_MonthlySalary",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tbl_SalaryPayment_tbl_VoucherType_VoucherTypeId",
                        column: x => x.VoucherTypeId,
                        principalTable: "tbl_VoucherType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeeSlipDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FeeSlipId = table.Column<int>(type: "int", nullable: false),
                    FeeTypeId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DiscountInAmount = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeSlipDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeeSlipDetails_FeeSlips_FeeSlipId",
                        column: x => x.FeeSlipId,
                        principalTable: "FeeSlips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeeSlipDetails_FeeTypes_FeeTypeId",
                        column: x => x.FeeTypeId,
                        principalTable: "FeeTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FeeSlipReceipts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FeeSlipId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaymentMethodId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EditBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EditeOn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MessageSent = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FeeSlipReceipts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FeeSlipReceipts_FeeSlipPaymentMethods_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalTable: "FeeSlipPaymentMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FeeSlipReceipts_FeeSlips_FeeSlipId",
                        column: x => x.FeeSlipId,
                        principalTable: "FeeSlips",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "HostelAttendances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AttendanceTypeId = table.Column<int>(type: "int", nullable: false),
                    HostelId = table.Column<int>(type: "int", nullable: false),
                    HostleAdmId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HostelAttendances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HostelAttendances_HostelAdmissions_HostleAdmId",
                        column: x => x.HostleAdmId,
                        principalTable: "HostelAdmissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_HostelAttendances_Hostel_HostelId",
                        column: x => x.HostelId,
                        principalTable: "Hostel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_HostelAttendances_StudentAttendanceTypes_AttendanceTypeId",
                        column: x => x.AttendanceTypeId,
                        principalTable: "StudentAttendanceTypes",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Visitors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CampusId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Relationship = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Cnic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Contact = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StudentRegNo = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    VistorType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EntryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HostelAdmissionId = table.Column<int>(type: "int", nullable: true),
                    HostelId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visitors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Visitors_HostelAdmissions_HostelAdmissionId",
                        column: x => x.HostelAdmissionId,
                        principalTable: "HostelAdmissions",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Visitors_Hostel_HostelId",
                        column: x => x.HostelId,
                        principalTable: "Hostel",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AddmissionWaitingLists_CampusID",
                table: "AddmissionWaitingLists",
                column: "CampusID");

            migrationBuilder.CreateIndex(
                name: "IX_AddmissionWaitingLists_ClassID",
                table: "AddmissionWaitingLists",
                column: "ClassID");

            migrationBuilder.CreateIndex(
                name: "IX_Admissions_CampuseID",
                table: "Admissions",
                column: "CampuseID");

            migrationBuilder.CreateIndex(
                name: "IX_Admissions_ClassSectionID",
                table: "Admissions",
                column: "ClassSectionID");

            migrationBuilder.CreateIndex(
                name: "IX_Admissions_StudentID",
                table: "Admissions",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationLeave_StaffId",
                table: "ApplicationLeave",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BudgetDetails_BudgetMasterId",
                table: "BudgetDetails",
                column: "BudgetMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_CallRecords_AdmissionID",
                table: "CallRecords",
                column: "AdmissionID");

            migrationBuilder.CreateIndex(
                name: "IX_CampusUsers_CampusID",
                table: "CampusUsers",
                column: "CampusID");

            migrationBuilder.CreateIndex(
                name: "IX_CampusUsers_UserID",
                table: "CampusUsers",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_ClassFeeGroups_CampusID",
                table: "ClassFeeGroups",
                column: "CampusID");

            migrationBuilder.CreateIndex(
                name: "IX_ClassFeeGroups_ClassID",
                table: "ClassFeeGroups",
                column: "ClassID");

            migrationBuilder.CreateIndex(
                name: "IX_ClassFeeGroups_FeeGroupID",
                table: "ClassFeeGroups",
                column: "FeeGroupID");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSections_CampusID",
                table: "ClassSections",
                column: "CampusID");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSections_ClassID",
                table: "ClassSections",
                column: "ClassID");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSections_SectionID",
                table: "ClassSections",
                column: "SectionID");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSubjects_CampusID",
                table: "ClassSubjects",
                column: "CampusID");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSubjects_ClassID",
                table: "ClassSubjects",
                column: "ClassID");

            migrationBuilder.CreateIndex(
                name: "IX_ClassSubjects_SubjectID",
                table: "ClassSubjects",
                column: "SubjectID");

            migrationBuilder.CreateIndex(
                name: "IX_ComSubs_StudentID",
                table: "ComSubs",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_DOBChanges_StudentID",
                table: "DOBChanges",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_DOBChanges_UserID",
                table: "DOBChanges",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Drivers_CampusID",
                table: "Drivers",
                column: "CampusID");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAttendance_AttendanceTypeID",
                table: "EmployeeAttendance",
                column: "AttendanceTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAttendance_StaffID",
                table: "EmployeeAttendance",
                column: "StaffID");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAttendance_UserID",
                table: "EmployeeAttendance",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeAwards_UserID",
                table: "EmployeeAwards",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeExperinceSubjects_EmployeeId",
                table: "EmployeeExperinceSubjects",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeExperinceSubjects_SubjectId",
                table: "EmployeeExperinceSubjects",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeExprience_StfID",
                table: "EmployeeExprience",
                column: "StfID");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShortLeave_StaffID",
                table: "EmployeeShortLeave",
                column: "StaffID");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeShortLeave_UserID",
                table: "EmployeeShortLeave",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeThumbImpressions_StaffID",
                table: "EmployeeThumbImpressions",
                column: "StaffID");

            migrationBuilder.CreateIndex(
                name: "IX_ExamDates_CampusID",
                table: "ExamDates",
                column: "CampusID");

            migrationBuilder.CreateIndex(
                name: "IX_ExamDates_ClassSectionID",
                table: "ExamDates",
                column: "ClassSectionID");

            migrationBuilder.CreateIndex(
                name: "IX_ExamDates_ExamHeldID",
                table: "ExamDates",
                column: "ExamHeldID");

            migrationBuilder.CreateIndex(
                name: "IX_ExamDates_SubjectID",
                table: "ExamDates",
                column: "SubjectID");

            migrationBuilder.CreateIndex(
                name: "IX_ExamHelds_CampusID",
                table: "ExamHelds",
                column: "CampusID");

            migrationBuilder.CreateIndex(
                name: "IX_ExamHelds_ExamID",
                table: "ExamHelds",
                column: "ExamID");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRemarks_AdmissionID",
                table: "ExamRemarks",
                column: "AdmissionID");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRemarks_ExamHeldID",
                table: "ExamRemarks",
                column: "ExamHeldID");

            migrationBuilder.CreateIndex(
                name: "IX_ExamRemarks_UserID",
                table: "ExamRemarks",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_ExamsRules_ExamHeldID",
                table: "ExamsRules",
                column: "ExamHeldID");

            migrationBuilder.CreateIndex(
                name: "IX_ExpellDetails_StudentID",
                table: "ExpellDetails",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_ExpellDetails_UserID",
                table: "ExpellDetails",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseDetails_ExpenseMasterId",
                table: "ExpenseDetails",
                column: "ExpenseMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseDetails_LedgerId",
                table: "ExpenseDetails",
                column: "LedgerId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseMasters_LedgerId",
                table: "ExpenseMasters",
                column: "LedgerId");

            migrationBuilder.CreateIndex(
                name: "IX_ExpenseMasters_VoucherTypeId",
                table: "ExpenseMasters",
                column: "VoucherTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeDiscounts_FeeTypeId",
                table: "FeeDiscounts",
                column: "FeeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeDiscounts_StudentId",
                table: "FeeDiscounts",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeSlipDetails_FeeSlipId",
                table: "FeeSlipDetails",
                column: "FeeSlipId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeSlipDetails_FeeTypeId",
                table: "FeeSlipDetails",
                column: "FeeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeSlipPaymentMethods_CampusID",
                table: "FeeSlipPaymentMethods",
                column: "CampusID");

            migrationBuilder.CreateIndex(
                name: "IX_FeeSlipReceipts_FeeSlipId",
                table: "FeeSlipReceipts",
                column: "FeeSlipId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeSlipReceipts_PaymentMethodId",
                table: "FeeSlipReceipts",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeSlips_AdmissionId",
                table: "FeeSlips",
                column: "AdmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeStructures_CampusID",
                table: "FeeStructures",
                column: "CampusID");

            migrationBuilder.CreateIndex(
                name: "IX_FeeStructures_FeeGroupId",
                table: "FeeStructures",
                column: "FeeGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_FeeStructures_FeeTypeId",
                table: "FeeStructures",
                column: "FeeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FineDeductions_UserID",
                table: "FineDeductions",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_FreeAdmissions_StudentID",
                table: "FreeAdmissions",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_Homework_ClassSectionId",
                table: "Homework",
                column: "ClassSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_Homework_SubjectId",
                table: "Homework",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Hostel_CampusId",
                table: "Hostel",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_HostelAdmissions_AdmisionId",
                table: "HostelAdmissions",
                column: "AdmisionId");

            migrationBuilder.CreateIndex(
                name: "IX_HostelAdmissions_HostelId",
                table: "HostelAdmissions",
                column: "HostelId");

            migrationBuilder.CreateIndex(
                name: "IX_HostelAdmissions_RoomId",
                table: "HostelAdmissions",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_HostelAttendances_AttendanceTypeId",
                table: "HostelAttendances",
                column: "AttendanceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_HostelAttendances_HostelId",
                table: "HostelAttendances",
                column: "HostelId");

            migrationBuilder.CreateIndex(
                name: "IX_HostelAttendances_HostleAdmId",
                table: "HostelAttendances",
                column: "HostleAdmId");

            migrationBuilder.CreateIndex(
                name: "IX_IssuedBoardCertificates_StdID",
                table: "IssuedBoardCertificates",
                column: "StdID");

            migrationBuilder.CreateIndex(
                name: "IX_IssuedBoardCertificates_UserID",
                table: "IssuedBoardCertificates",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_IssuedBooks_BookID",
                table: "IssuedBooks",
                column: "BookID");

            migrationBuilder.CreateIndex(
                name: "IX_IssuedBooks_StudentID",
                table: "IssuedBooks",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_IssuedBooks_UserID",
                table: "IssuedBooks",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_IssuedBooksToStaffs_UserID",
                table: "IssuedBooksToStaffs",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_IssuedCertificates_StudentID",
                table: "IssuedCertificates",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_IssuedCertificates_UserID",
                table: "IssuedCertificates",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_JournalDetails_JournalMasterId",
                table: "JournalDetails",
                column: "JournalMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalDetails_LedgerId",
                table: "JournalDetails",
                column: "LedgerId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalMasters_LedgerId",
                table: "JournalMasters",
                column: "LedgerId");

            migrationBuilder.CreateIndex(
                name: "IX_JournalMasters_VoucherTypeId",
                table: "JournalMasters",
                column: "VoucherTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeavedStaffs_StaffID",
                table: "LeavedStaffs",
                column: "StaffID");

            migrationBuilder.CreateIndex(
                name: "IX_LeavedStaffs_UserID",
                table: "LeavedStaffs",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Libraries_CampusId",
                table: "Libraries",
                column: "CampusId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryBookRecords_LibraryBookListId",
                table: "LibraryBookRecords",
                column: "LibraryBookListId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryBookRecords_LibraryId",
                table: "LibraryBookRecords",
                column: "LibraryId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryIssuedBooks_AspNetUserId",
                table: "LibraryIssuedBooks",
                column: "AspNetUserId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryIssuedBooks_LibraryBookListID",
                table: "LibraryIssuedBooks",
                column: "LibraryBookListID");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryIssuedBooksToStaffs_LibraryBookRecordId",
                table: "LibraryIssuedBooksToStaffs",
                column: "LibraryBookRecordId");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryIssuedBooksToStaffs_UserID",
                table: "LibraryIssuedBooksToStaffs",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_LibraryListOfBooks_ClassID",
                table: "LibraryListOfBooks",
                column: "ClassID");

            migrationBuilder.CreateIndex(
                name: "IX_ListOfBooks_ClassID",
                table: "ListOfBooks",
                column: "ClassID");

            migrationBuilder.CreateIndex(
                name: "IX_MenuItems_ActionId",
                table: "MenuItems",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_MessageRecords_StudentID",
                table: "MessageRecords",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_MessageRecords_UserID",
                table: "MessageRecords",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_MobileMessages_FromId",
                table: "MobileMessages",
                column: "FromId");

            migrationBuilder.CreateIndex(
                name: "IX_OldJinnahians_StudentID",
                table: "OldJinnahians",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_ParentsVisits_StudentID",
                table: "ParentsVisits",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDetails_LedgerId",
                table: "PaymentDetails",
                column: "LedgerId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentDetails_PaymentMasterId",
                table: "PaymentDetails",
                column: "PaymentMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMasters_LedgerId",
                table: "PaymentMasters",
                column: "LedgerId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMasters_VoucherTypeId",
                table: "PaymentMasters",
                column: "VoucherTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Privlidges_ActionId",
                table: "Privlidges",
                column: "ActionId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfessionalTrainings_StaffID",
                table: "ProfessionalTrainings",
                column: "StaffID");

            migrationBuilder.CreateIndex(
                name: "IX_ProfessionalTrainings_UserID",
                table: "ProfessionalTrainings",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_ProhibitedMaterials_StudentID",
                table: "ProhibitedMaterials",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_PublicPosts_PostCategoryId",
                table: "PublicPosts",
                column: "PostCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Qualifications_StaffID",
                table: "Qualifications",
                column: "StaffID");

            migrationBuilder.CreateIndex(
                name: "IX_Qualifications_UserID",
                table: "Qualifications",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_RatingMasters_CampusID",
                table: "RatingMasters",
                column: "CampusID");

            migrationBuilder.CreateIndex(
                name: "IX_RatingParticipants_AdmissionID",
                table: "RatingParticipants",
                column: "AdmissionID");

            migrationBuilder.CreateIndex(
                name: "IX_RatingParticipants_MasterID",
                table: "RatingParticipants",
                column: "MasterID");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_MasterID",
                table: "Ratings",
                column: "MasterID");

            migrationBuilder.CreateIndex(
                name: "IX_ReAdmissions_AdmissionID",
                table: "ReAdmissions",
                column: "AdmissionID");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptDetails_LedgerId",
                table: "ReceiptDetails",
                column: "LedgerId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptDetails_ReceiptMasterId",
                table: "ReceiptDetails",
                column: "ReceiptMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptMasters_LedgerId",
                table: "ReceiptMasters",
                column: "LedgerId");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptMasters_VoucherTypeId",
                table: "ReceiptMasters",
                column: "VoucherTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Results_AdmissionID",
                table: "Results",
                column: "AdmissionID");

            migrationBuilder.CreateIndex(
                name: "IX_Results_ClassSubjectID",
                table: "Results",
                column: "ClassSubjectID");

            migrationBuilder.CreateIndex(
                name: "IX_Results_ExamHeldID",
                table: "Results",
                column: "ExamHeldID");

            migrationBuilder.CreateIndex(
                name: "IX_Results_UserID",
                table: "Results",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_ResultSummaries_ExamHeldID",
                table: "ResultSummaries",
                column: "ExamHeldID");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_HostelId",
                table: "Rooms",
                column: "HostelId");

            migrationBuilder.CreateIndex(
                name: "IX_SchoolLeaveSchedule_CampusID",
                table: "SchoolLeaveSchedule",
                column: "CampusID");

            migrationBuilder.CreateIndex(
                name: "IX_ScienceLabItemsStocks_CampusID",
                table: "ScienceLabItemsStocks",
                column: "CampusID");

            migrationBuilder.CreateIndex(
                name: "IX_ScienceLabItemsStocks_LIID",
                table: "ScienceLabItemsStocks",
                column: "LIID");

            migrationBuilder.CreateIndex(
                name: "IX_ScienceLabMissingItems_CampusID",
                table: "ScienceLabMissingItems",
                column: "CampusID");

            migrationBuilder.CreateIndex(
                name: "IX_ScienceLabMissingItems_LIID",
                table: "ScienceLabMissingItems",
                column: "LIID");

            migrationBuilder.CreateIndex(
                name: "IX_SelectedForCadetColleges_StudentID",
                table: "SelectedForCadetColleges",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_SelectedForCadetColleges_UserID",
                table: "SelectedForCadetColleges",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_SignatureSpeciman_StudentID",
                table: "SignatureSpeciman",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_SLCs_ExpellDetailID",
                table: "SLCs",
                column: "ExpellDetailID");

            migrationBuilder.CreateIndex(
                name: "IX_SMSApplicationProperty_CampusID",
                table: "SMSApplicationProperty",
                column: "CampusID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAlumnis_StudentID",
                table: "StudentAlumnis",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAttendences_AdmissionID",
                table: "StudentAttendences",
                column: "AdmissionID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAttendences_AttendanceTypeID",
                table: "StudentAttendences",
                column: "AttendanceTypeID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAttendences_UserID",
                table: "StudentAttendences",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentComplaigns_StudentID",
                table: "StudentComplaigns",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentMobiles_StudentID",
                table: "StudentMobiles",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentPhotos_StudentID",
                table: "StudentPhotos",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentPreviousEducations_AdmissionID",
                table: "StudentPreviousEducations",
                column: "AdmissionID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentsTransports_DriverID",
                table: "StudentsTransports",
                column: "DriverID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentsTransports_StudentID",
                table: "StudentsTransports",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentsWarnings_StudentID",
                table: "StudentsWarnings",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_StudentThumbImpressions_StudentID",
                table: "StudentThumbImpressions",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_AccountLedger_AccountGroupId",
                table: "tbl_AccountLedger",
                column: "AccountGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Employee_CampusID",
                table: "tbl_Employee",
                column: "CampusID");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_Employee_designationId",
                table: "tbl_Employee",
                column: "designationId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_MonthlySalary_EmployeeId",
                table: "tbl_MonthlySalary",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_MonthlySalary_SalaryPackageId",
                table: "tbl_MonthlySalary",
                column: "SalaryPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_MonthlySalaryDetails_MonthlySalaryId",
                table: "tbl_MonthlySalaryDetails",
                column: "MonthlySalaryId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_MonthlySalaryDetails_PayHeadId",
                table: "tbl_MonthlySalaryDetails",
                column: "PayHeadId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SalaryPackageDetails_payHeadId",
                table: "tbl_SalaryPackageDetails",
                column: "payHeadId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SalaryPackageDetails_salaryPackageId",
                table: "tbl_SalaryPackageDetails",
                column: "salaryPackageId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SalaryPayment_LedgerId",
                table: "tbl_SalaryPayment",
                column: "LedgerId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SalaryPayment_MonthlySalaryId",
                table: "tbl_SalaryPayment",
                column: "MonthlySalaryId");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_SalaryPayment_VoucherTypeId",
                table: "tbl_SalaryPayment",
                column: "VoucherTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingClasses_ClassSectionId",
                table: "TeachingClasses",
                column: "ClassSectionId");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingSubject_CampusID",
                table: "TeachingSubject",
                column: "CampusID");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingSubject_ClassSectionID",
                table: "TeachingSubject",
                column: "ClassSectionID");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingSubject_StaffID",
                table: "TeachingSubject",
                column: "StaffID");

            migrationBuilder.CreateIndex(
                name: "IX_TeachingSubject_SubjectId",
                table: "TeachingSubject",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeConsumptionForResultEntries_UserID",
                table: "TimeConsumptionForResultEntries",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_UnwantedClients_StudentID",
                table: "UnwantedClients",
                column: "StudentID");

            migrationBuilder.CreateIndex(
                name: "IX_UnwantedClients_UserID",
                table: "UnwantedClients",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Visitors_HostelAdmissionId",
                table: "Visitors",
                column: "HostelAdmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Visitors_HostelId",
                table: "Visitors",
                column: "HostelId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ActiveRatings");

            migrationBuilder.DropTable(
                name: "AddmissionWaitingLists");

            migrationBuilder.DropTable(
                name: "ApplicationLeave");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AssetStocks");

            migrationBuilder.DropTable(
                name: "BudgetDetails");

            migrationBuilder.DropTable(
                name: "CallRecords");

            migrationBuilder.DropTable(
                name: "CampusUsers");

            migrationBuilder.DropTable(
                name: "ClassFeeGroups");

            migrationBuilder.DropTable(
                name: "ComSubs");

            migrationBuilder.DropTable(
                name: "DailyCashes");

            migrationBuilder.DropTable(
                name: "DOBChanges");

            migrationBuilder.DropTable(
                name: "EmployeeAttendance");

            migrationBuilder.DropTable(
                name: "EmployeeAwards");

            migrationBuilder.DropTable(
                name: "EmployeeExperinceSubjects");

            migrationBuilder.DropTable(
                name: "EmployeeExprience");

            migrationBuilder.DropTable(
                name: "EmployeeMobiles");

            migrationBuilder.DropTable(
                name: "EmployeeShortLeave");

            migrationBuilder.DropTable(
                name: "EmployeeThumbImpressions");

            migrationBuilder.DropTable(
                name: "EmployeeWaitingLists");

            migrationBuilder.DropTable(
                name: "ExamDates");

            migrationBuilder.DropTable(
                name: "ExamRemarks");

            migrationBuilder.DropTable(
                name: "ExamRemarksDetail");

            migrationBuilder.DropTable(
                name: "ExamsRules");

            migrationBuilder.DropTable(
                name: "ExpenseDetails");

            migrationBuilder.DropTable(
                name: "FeeDiscounts");

            migrationBuilder.DropTable(
                name: "FeeSlipDetails");

            migrationBuilder.DropTable(
                name: "FeeSlipReceipts");

            migrationBuilder.DropTable(
                name: "FeeStructures");

            migrationBuilder.DropTable(
                name: "FineDeductions");

            migrationBuilder.DropTable(
                name: "FreeAdmissions");

            migrationBuilder.DropTable(
                name: "Homework");

            migrationBuilder.DropTable(
                name: "HostelAttendances");

            migrationBuilder.DropTable(
                name: "IssuedBoardCertificates");

            migrationBuilder.DropTable(
                name: "IssuedBooks");

            migrationBuilder.DropTable(
                name: "IssuedBooksToStaffs");

            migrationBuilder.DropTable(
                name: "IssuedCertificates");

            migrationBuilder.DropTable(
                name: "JournalDetails");

            migrationBuilder.DropTable(
                name: "LeavedStaffs");

            migrationBuilder.DropTable(
                name: "LedgerPostings");

            migrationBuilder.DropTable(
                name: "LibraryIssuedBooks");

            migrationBuilder.DropTable(
                name: "LibraryIssuedBooksToStaffs");

            migrationBuilder.DropTable(
                name: "LibraryListOfBooks");

            migrationBuilder.DropTable(
                name: "ListOfBooks");

            migrationBuilder.DropTable(
                name: "MenuItems");

            migrationBuilder.DropTable(
                name: "MessageRecords");

            migrationBuilder.DropTable(
                name: "MobileDevices");

            migrationBuilder.DropTable(
                name: "MobileMessages");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "OldJinnahians");

            migrationBuilder.DropTable(
                name: "ParentsVisits");

            migrationBuilder.DropTable(
                name: "PaymentDetails");

            migrationBuilder.DropTable(
                name: "PeriodMasters");

            migrationBuilder.DropTable(
                name: "Privlidges");

            migrationBuilder.DropTable(
                name: "ProfessionalTrainings");

            migrationBuilder.DropTable(
                name: "ProhibitedMaterials");

            migrationBuilder.DropTable(
                name: "PublicMenus");

            migrationBuilder.DropTable(
                name: "PublicMobiles");

            migrationBuilder.DropTable(
                name: "PublicPages");

            migrationBuilder.DropTable(
                name: "PublicPosts");

            migrationBuilder.DropTable(
                name: "PublicSliders");

            migrationBuilder.DropTable(
                name: "Qualifications");

            migrationBuilder.DropTable(
                name: "RatingParticipants");

            migrationBuilder.DropTable(
                name: "Ratings");

            migrationBuilder.DropTable(
                name: "ReAdmissions");

            migrationBuilder.DropTable(
                name: "ReceiptDetails");

            migrationBuilder.DropTable(
                name: "Results");

            migrationBuilder.DropTable(
                name: "ResultSummaries");

            migrationBuilder.DropTable(
                name: "SchoolInfoes");

            migrationBuilder.DropTable(
                name: "SchoolLeaveSchedule");

            migrationBuilder.DropTable(
                name: "SchoolLogo");

            migrationBuilder.DropTable(
                name: "ScienceLabItemsStocks");

            migrationBuilder.DropTable(
                name: "ScienceLabMissingItems");

            migrationBuilder.DropTable(
                name: "SelectedForCadetColleges");

            migrationBuilder.DropTable(
                name: "SignatureSpeciman");

            migrationBuilder.DropTable(
                name: "SLCs");

            migrationBuilder.DropTable(
                name: "SMSApplicationProperty");

            migrationBuilder.DropTable(
                name: "StudentAlumnis");

            migrationBuilder.DropTable(
                name: "StudentAttendences");

            migrationBuilder.DropTable(
                name: "StudentComplaigns");

            migrationBuilder.DropTable(
                name: "StudentMobiles");

            migrationBuilder.DropTable(
                name: "StudentPhotos");

            migrationBuilder.DropTable(
                name: "StudentPreviousEducations");

            migrationBuilder.DropTable(
                name: "StudentsTransports");

            migrationBuilder.DropTable(
                name: "StudentsWarnings");

            migrationBuilder.DropTable(
                name: "StudentThumbImpressions");

            migrationBuilder.DropTable(
                name: "TaskDairies");

            migrationBuilder.DropTable(
                name: "tbl_Company");

            migrationBuilder.DropTable(
                name: "tbl_FinancialYear");

            migrationBuilder.DropTable(
                name: "tbl_MonthlySalaryDetails");

            migrationBuilder.DropTable(
                name: "tbl_SalaryPackageDetails");

            migrationBuilder.DropTable(
                name: "tbl_SalaryPayment");

            migrationBuilder.DropTable(
                name: "TeachingClasses");

            migrationBuilder.DropTable(
                name: "TeachingSubject");

            migrationBuilder.DropTable(
                name: "TimeConsumptionForResultEntries");

            migrationBuilder.DropTable(
                name: "TimeTable");

            migrationBuilder.DropTable(
                name: "UnwantedClients");

            migrationBuilder.DropTable(
                name: "UserDefinitions");

            migrationBuilder.DropTable(
                name: "Visitors");

            migrationBuilder.DropTable(
                name: "VoteByStudents");

            migrationBuilder.DropTable(
                name: "WebSites");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "BudgetMasters");

            migrationBuilder.DropTable(
                name: "EmployeeAttendanceType");

            migrationBuilder.DropTable(
                name: "ExpenseMasters");

            migrationBuilder.DropTable(
                name: "FeeSlipPaymentMethods");

            migrationBuilder.DropTable(
                name: "FeeSlips");

            migrationBuilder.DropTable(
                name: "FeeGroups");

            migrationBuilder.DropTable(
                name: "FeeTypes");

            migrationBuilder.DropTable(
                name: "Books");

            migrationBuilder.DropTable(
                name: "JournalMasters");

            migrationBuilder.DropTable(
                name: "LibraryBookRecords");

            migrationBuilder.DropTable(
                name: "PaymentMasters");

            migrationBuilder.DropTable(
                name: "MenuActions");

            migrationBuilder.DropTable(
                name: "PublicPostCategories");

            migrationBuilder.DropTable(
                name: "RatingMasters");

            migrationBuilder.DropTable(
                name: "ReceiptMasters");

            migrationBuilder.DropTable(
                name: "ClassSubjects");

            migrationBuilder.DropTable(
                name: "ExamHelds");

            migrationBuilder.DropTable(
                name: "ScienceLabItems");

            migrationBuilder.DropTable(
                name: "ExpellDetails");

            migrationBuilder.DropTable(
                name: "StudentAttendanceTypes");

            migrationBuilder.DropTable(
                name: "Drivers");

            migrationBuilder.DropTable(
                name: "tbl_PayHead");

            migrationBuilder.DropTable(
                name: "tbl_MonthlySalary");

            migrationBuilder.DropTable(
                name: "HostelAdmissions");

            migrationBuilder.DropTable(
                name: "Libraries");

            migrationBuilder.DropTable(
                name: "LibraryBookLists");

            migrationBuilder.DropTable(
                name: "tbl_AccountLedger");

            migrationBuilder.DropTable(
                name: "tbl_VoucherType");

            migrationBuilder.DropTable(
                name: "Subjects");

            migrationBuilder.DropTable(
                name: "Exams");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "tbl_Employee");

            migrationBuilder.DropTable(
                name: "tbl_SalaryPackage");

            migrationBuilder.DropTable(
                name: "Admissions");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "tbl_AccountGroup");

            migrationBuilder.DropTable(
                name: "tbl_Designation");

            migrationBuilder.DropTable(
                name: "ClassSections");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Hostel");

            migrationBuilder.DropTable(
                name: "Classes");

            migrationBuilder.DropTable(
                name: "Sections");

            migrationBuilder.DropTable(
                name: "Campuses");
        }
    }
}
