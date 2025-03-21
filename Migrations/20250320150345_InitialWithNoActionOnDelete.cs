using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MuhasebeStokWebApp.Migrations
{
    /// <inheritdoc />
    public partial class InitialWithNoActionOnDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                name: "Bankalar",
                columns: table => new
                {
                    BankaID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BankaAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SubeAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SubeKodu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HesapNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IBAN = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ParaBirimi = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false, defaultValue: "TRY"),
                    AcilisBakiye = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    GuncelBakiye = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    YetkiliKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OlusturanKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SonGuncelleyenKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SoftDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bankalar", x => x.BankaID);
                });

            migrationBuilder.CreateTable(
                name: "Birimler",
                columns: table => new
                {
                    BirimID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BirimAdi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    SoftDelete = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Birimler", x => x.BirimID);
                });

            migrationBuilder.CreateTable(
                name: "Cariler",
                columns: table => new
                {
                    CariID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CariAdi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    VergiNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Telefon = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    OlusturanKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SonGuncelleyenKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SoftDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Adres = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Yetkili = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cariler", x => x.CariID);
                });

            migrationBuilder.CreateTable(
                name: "Depolar",
                columns: table => new
                {
                    DepoID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepoAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Adres = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    OlusturanKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SonGuncelleyenKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SoftDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Depolar", x => x.DepoID);
                });

            migrationBuilder.CreateTable(
                name: "DovizKurlari",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ParaBirimi = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    BazParaBirimi = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Alis = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Satis = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    EfektifAlis = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    EfektifSatis = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Kaynak = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SoftDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DovizKurlari", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "FaturaTurleri",
                columns: table => new
                {
                    FaturaTuruID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FaturaTuruAdi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HareketTuru = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaturaTurleri", x => x.FaturaTuruID);
                });

            migrationBuilder.CreateTable(
                name: "FiyatTipleri",
                columns: table => new
                {
                    FiyatTipiID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TipAdi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FiyatTipleri", x => x.FiyatTipiID);
                });

            migrationBuilder.CreateTable(
                name: "IrsaliyeTurleri",
                columns: table => new
                {
                    IrsaliyeTuruID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IrsaliyeTuruAdi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    HareketTuru = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IrsaliyeTurleri", x => x.IrsaliyeTuruID);
                });

            migrationBuilder.CreateTable(
                name: "Kasalar",
                columns: table => new
                {
                    KasaID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KasaAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    KasaTuru = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ParaBirimi = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    AcilisBakiye = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    GuncelBakiye = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SorumluKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OlusturanKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SonGuncelleyenKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SoftDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kasalar", x => x.KasaID);
                });

            migrationBuilder.CreateTable(
                name: "OdemeTurleri",
                columns: table => new
                {
                    OdemeTuruID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OdemeTuruAdi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OdemeTurleri", x => x.OdemeTuruID);
                });

            migrationBuilder.CreateTable(
                name: "ParaBirimi",
                columns: table => new
                {
                    ParaBirimiID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DovizKodu = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    DovizAdi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Sembol = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Silindi = table.Column<bool>(type: "bit", nullable: false),
                    Sira = table.Column<int>(type: "int", nullable: false),
                    Format = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParaBirimi", x => x.ParaBirimiID);
                });

            migrationBuilder.CreateTable(
                name: "SistemAyarlari",
                columns: table => new
                {
                    SistemAyarlariID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AnaDovizKodu = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    SirketAdi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SirketAdresi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SirketTelefon = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SirketEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SirketVergiNo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SirketVergiDairesi = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    OtomatikDovizGuncelleme = table.Column<bool>(type: "bit", nullable: false),
                    DovizGuncellemeSikligi = table.Column<int>(type: "int", nullable: false),
                    SonDovizGuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AktifParaBirimleri = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    SoftDelete = table.Column<bool>(type: "bit", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SistemAyarlari", x => x.SistemAyarlariID);
                });

            migrationBuilder.CreateTable(
                name: "SistemLoglar",
                columns: table => new
                {
                    LogID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IslemTuru = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LogTuru = table.Column<int>(type: "int", nullable: false),
                    KayitID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TabloAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    KayitAdi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IslemTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    KullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    KullaniciAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IPAdresi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Basarili = table.Column<bool>(type: "bit", nullable: false),
                    HataMesaji = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SistemLoglar", x => x.LogID);
                });

            migrationBuilder.CreateTable(
                name: "UrunKategorileri",
                columns: table => new
                {
                    KategoriID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KategoriAdi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Aktif = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    OlusturanKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SonGuncelleyenKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SoftDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunKategorileri", x => x.KategoriID);
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
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
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
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
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
                name: "CariHareketler",
                columns: table => new
                {
                    CariHareketID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CariID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HareketTuru = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReferansNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReferansTuru = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ReferansID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SoftDelete = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CariHareketler", x => x.CariHareketID);
                    table.ForeignKey(
                        name: "FK_CariHareketler_Cariler_CariID",
                        column: x => x.CariID,
                        principalTable: "Cariler",
                        principalColumn: "CariID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BankaHareketleri",
                columns: table => new
                {
                    BankaHareketID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BankaID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KaynakKasaID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HedefKasaID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TransferID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CariID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Tutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HareketTuru = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReferansNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReferansTuru = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReferansID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    DekontNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IslemYapanKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SonGuncelleyenKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SoftDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    KarsiUnvan = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    KarsiBankaAdi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    KarsiIBAN = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BankaHareketleri", x => x.BankaHareketID);
                    table.ForeignKey(
                        name: "FK_BankaHareketleri_Bankalar_BankaID",
                        column: x => x.BankaID,
                        principalTable: "Bankalar",
                        principalColumn: "BankaID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BankaHareketleri_Cariler_CariID",
                        column: x => x.CariID,
                        principalTable: "Cariler",
                        principalColumn: "CariID");
                    table.ForeignKey(
                        name: "FK_BankaHareketleri_Kasalar_HedefKasaID",
                        column: x => x.HedefKasaID,
                        principalTable: "Kasalar",
                        principalColumn: "KasaID");
                    table.ForeignKey(
                        name: "FK_BankaHareketleri_Kasalar_KaynakKasaID",
                        column: x => x.KaynakKasaID,
                        principalTable: "Kasalar",
                        principalColumn: "KasaID");
                });

            migrationBuilder.CreateTable(
                name: "KasaHareketleri",
                columns: table => new
                {
                    KasaHareketID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KasaID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CariID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    KaynakBankaID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Tutar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    HareketTuru = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    HedefKasaID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    HedefBankaID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IslemTuru = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    DovizKuru = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    KarsiParaBirimi = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReferansNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReferansTuru = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReferansID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IslemYapanKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SonGuncelleyenKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SoftDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    TransferID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KasaHareketleri", x => x.KasaHareketID);
                    table.ForeignKey(
                        name: "FK_KasaHareketleri_Bankalar_HedefBankaID",
                        column: x => x.HedefBankaID,
                        principalTable: "Bankalar",
                        principalColumn: "BankaID");
                    table.ForeignKey(
                        name: "FK_KasaHareketleri_Bankalar_KaynakBankaID",
                        column: x => x.KaynakBankaID,
                        principalTable: "Bankalar",
                        principalColumn: "BankaID");
                    table.ForeignKey(
                        name: "FK_KasaHareketleri_Cariler_CariID",
                        column: x => x.CariID,
                        principalTable: "Cariler",
                        principalColumn: "CariID");
                    table.ForeignKey(
                        name: "FK_KasaHareketleri_Kasalar_HedefKasaID",
                        column: x => x.HedefKasaID,
                        principalTable: "Kasalar",
                        principalColumn: "KasaID");
                    table.ForeignKey(
                        name: "FK_KasaHareketleri_Kasalar_KasaID",
                        column: x => x.KasaID,
                        principalTable: "Kasalar",
                        principalColumn: "KasaID");
                });

            migrationBuilder.CreateTable(
                name: "Faturalar",
                columns: table => new
                {
                    FaturaID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FaturaNumarasi = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SiparisNumarasi = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FaturaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    VadeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CariID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FaturaTuruID = table.Column<int>(type: "int", nullable: true),
                    AraToplam = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    KDVToplam = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    GenelToplam = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    OdemeDurumu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FaturaNotu = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Resmi = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    DovizTuru = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    DovizKuru = table.Column<decimal>(type: "decimal(18,4)", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SoftDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    OlusturanKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SonGuncelleyenKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OdemeTuruID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Faturalar", x => x.FaturaID);
                    table.ForeignKey(
                        name: "FK_Faturalar_Cariler_CariID",
                        column: x => x.CariID,
                        principalTable: "Cariler",
                        principalColumn: "CariID");
                    table.ForeignKey(
                        name: "FK_Faturalar_FaturaTurleri_FaturaTuruID",
                        column: x => x.FaturaTuruID,
                        principalTable: "FaturaTurleri",
                        principalColumn: "FaturaTuruID");
                    table.ForeignKey(
                        name: "FK_Faturalar_OdemeTurleri_OdemeTuruID",
                        column: x => x.OdemeTuruID,
                        principalTable: "OdemeTurleri",
                        principalColumn: "OdemeTuruID");
                });

            migrationBuilder.CreateTable(
                name: "KurDegeri",
                columns: table => new
                {
                    KurDegeriID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ParaBirimiID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AlisDegeri = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SatisDegeri = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Kaynak = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    VeriKaynagi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Silindi = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KurDegeri", x => x.KurDegeriID);
                    table.ForeignKey(
                        name: "FK_KurDegeri_ParaBirimi_ParaBirimiID",
                        column: x => x.ParaBirimiID,
                        principalTable: "ParaBirimi",
                        principalColumn: "ParaBirimiID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ParaBirimiIliski",
                columns: table => new
                {
                    ParaBirimiIliskiID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KaynakParaBirimiID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HedefParaBirimiID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Carpan = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Silindi = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParaBirimiIliski", x => x.ParaBirimiIliskiID);
                    table.ForeignKey(
                        name: "FK_ParaBirimiIliski_ParaBirimi_HedefParaBirimiID",
                        column: x => x.HedefParaBirimiID,
                        principalTable: "ParaBirimi",
                        principalColumn: "ParaBirimiID");
                    table.ForeignKey(
                        name: "FK_ParaBirimiIliski_ParaBirimi_KaynakParaBirimiID",
                        column: x => x.KaynakParaBirimiID,
                        principalTable: "ParaBirimi",
                        principalColumn: "ParaBirimiID");
                });

            migrationBuilder.CreateTable(
                name: "Urunler",
                columns: table => new
                {
                    UrunID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UrunKodu = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UrunAdi = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BirimID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StokMiktar = table.Column<decimal>(type: "decimal(18,2)", nullable: false, defaultValue: 0m),
                    Aktif = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    KDVOrani = table.Column<int>(type: "int", nullable: false),
                    OlusturanKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SonGuncelleyenKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SoftDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    KategoriID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Urunler", x => x.UrunID);
                    table.ForeignKey(
                        name: "FK_Urunler_Birimler_BirimID",
                        column: x => x.BirimID,
                        principalTable: "Birimler",
                        principalColumn: "BirimID");
                    table.ForeignKey(
                        name: "FK_Urunler_UrunKategorileri_KategoriID",
                        column: x => x.KategoriID,
                        principalTable: "UrunKategorileri",
                        principalColumn: "KategoriID");
                });

            migrationBuilder.CreateTable(
                name: "FaturaOdemeleri",
                columns: table => new
                {
                    OdemeID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FaturaID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OdemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OdemeTutari = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    OdemeTuru = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OlusturanKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SonGuncelleyenKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SoftDelete = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaturaOdemeleri", x => x.OdemeID);
                    table.ForeignKey(
                        name: "FK_FaturaOdemeleri_Faturalar_FaturaID",
                        column: x => x.FaturaID,
                        principalTable: "Faturalar",
                        principalColumn: "FaturaID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Irsaliyeler",
                columns: table => new
                {
                    IrsaliyeID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FaturaID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IrsaliyeTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OlusturanKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SonGuncelleyenKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Aktif = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Resmi = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    IrsaliyeNumarasi = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CariID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IrsaliyeTuru = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Durum = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SoftDelete = table.Column<bool>(type: "bit", nullable: false),
                    IrsaliyeTuruID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Irsaliyeler", x => x.IrsaliyeID);
                    table.ForeignKey(
                        name: "FK_Irsaliyeler_Cariler_CariID",
                        column: x => x.CariID,
                        principalTable: "Cariler",
                        principalColumn: "CariID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Irsaliyeler_Faturalar_FaturaID",
                        column: x => x.FaturaID,
                        principalTable: "Faturalar",
                        principalColumn: "FaturaID");
                    table.ForeignKey(
                        name: "FK_Irsaliyeler_IrsaliyeTurleri_IrsaliyeTuruID",
                        column: x => x.IrsaliyeTuruID,
                        principalTable: "IrsaliyeTurleri",
                        principalColumn: "IrsaliyeTuruID");
                });

            migrationBuilder.CreateTable(
                name: "FaturaDetaylari",
                columns: table => new
                {
                    FaturaDetayID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FaturaID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UrunID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Miktar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BirimFiyat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KdvOrani = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    IndirimOrani = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SatirToplam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SatirKdvToplam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Birim = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    OlusturanKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SonGuncelleyenKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SoftDelete = table.Column<bool>(type: "bit", nullable: false),
                    Tutar = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    KdvTutari = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IndirimTutari = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    NetTutar = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    BirimID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaturaDetaylari", x => x.FaturaDetayID);
                    table.ForeignKey(
                        name: "FK_FaturaDetaylari_Birimler_BirimID",
                        column: x => x.BirimID,
                        principalTable: "Birimler",
                        principalColumn: "BirimID");
                    table.ForeignKey(
                        name: "FK_FaturaDetaylari_Faturalar_FaturaID",
                        column: x => x.FaturaID,
                        principalTable: "Faturalar",
                        principalColumn: "FaturaID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FaturaDetaylari_Urunler_UrunID",
                        column: x => x.UrunID,
                        principalTable: "Urunler",
                        principalColumn: "UrunID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StokFifo",
                columns: table => new
                {
                    StokFifoID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UrunID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Miktar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    KalanMiktar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BirimFiyat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Birim = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ParaBirimi = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    DovizKuru = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    USDBirimFiyat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TLBirimFiyat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UZSBirimFiyat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GirisTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SonCikisTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReferansNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReferansTuru = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ReferansID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    Iptal = table.Column<bool>(type: "bit", nullable: false),
                    IptalTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IptalAciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IptalEdenKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SoftDelete = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StokFifo", x => x.StokFifoID);
                    table.ForeignKey(
                        name: "FK_StokFifo_Urunler_UrunID",
                        column: x => x.UrunID,
                        principalTable: "Urunler",
                        principalColumn: "UrunID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StokHareketleri",
                columns: table => new
                {
                    StokHareketID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UrunID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepoID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Miktar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Birim = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HareketTuru = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReferansNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReferansTuru = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReferansID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Aciklama = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BirimFiyat = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IslemYapanKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SonGuncelleyenKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SoftDelete = table.Column<bool>(type: "bit", nullable: false),
                    FaturaID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IrsaliyeID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StokHareketleri", x => x.StokHareketID);
                    table.ForeignKey(
                        name: "FK_StokHareketleri_Depolar_DepoID",
                        column: x => x.DepoID,
                        principalTable: "Depolar",
                        principalColumn: "DepoID");
                    table.ForeignKey(
                        name: "FK_StokHareketleri_Urunler_UrunID",
                        column: x => x.UrunID,
                        principalTable: "Urunler",
                        principalColumn: "UrunID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UrunFiyatlari",
                columns: table => new
                {
                    FiyatID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UrunID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Fiyat = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    GecerliTarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FiyatTipiID = table.Column<int>(type: "int", nullable: true),
                    OlusturanKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SonGuncelleyenKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SoftDelete = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UrunFiyatlari", x => x.FiyatID);
                    table.ForeignKey(
                        name: "FK_UrunFiyatlari_FiyatTipleri_FiyatTipiID",
                        column: x => x.FiyatTipiID,
                        principalTable: "FiyatTipleri",
                        principalColumn: "FiyatTipiID");
                    table.ForeignKey(
                        name: "FK_UrunFiyatlari_Urunler_UrunID",
                        column: x => x.UrunID,
                        principalTable: "Urunler",
                        principalColumn: "UrunID");
                });

            migrationBuilder.CreateTable(
                name: "IrsaliyeDetaylari",
                columns: table => new
                {
                    IrsaliyeDetayID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IrsaliyeID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UrunID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Miktar = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Birim = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Aciklama = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    OlusturanKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    SonGuncelleyenKullaniciID = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OlusturmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GuncellemeTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Aktif = table.Column<bool>(type: "bit", nullable: false),
                    SoftDelete = table.Column<bool>(type: "bit", nullable: false),
                    SatirToplam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SatirKdvToplam = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    BirimID = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IrsaliyeDetaylari", x => x.IrsaliyeDetayID);
                    table.ForeignKey(
                        name: "FK_IrsaliyeDetaylari_Birimler_BirimID",
                        column: x => x.BirimID,
                        principalTable: "Birimler",
                        principalColumn: "BirimID");
                    table.ForeignKey(
                        name: "FK_IrsaliyeDetaylari_Irsaliyeler_IrsaliyeID",
                        column: x => x.IrsaliyeID,
                        principalTable: "Irsaliyeler",
                        principalColumn: "IrsaliyeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_IrsaliyeDetaylari_Urunler_UrunID",
                        column: x => x.UrunID,
                        principalTable: "Urunler",
                        principalColumn: "UrunID",
                        onDelete: ReferentialAction.Cascade);
                });

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
                name: "IX_BankaHareketleri_BankaID",
                table: "BankaHareketleri",
                column: "BankaID");

            migrationBuilder.CreateIndex(
                name: "IX_BankaHareketleri_CariID",
                table: "BankaHareketleri",
                column: "CariID");

            migrationBuilder.CreateIndex(
                name: "IX_BankaHareketleri_HedefKasaID",
                table: "BankaHareketleri",
                column: "HedefKasaID");

            migrationBuilder.CreateIndex(
                name: "IX_BankaHareketleri_KaynakKasaID",
                table: "BankaHareketleri",
                column: "KaynakKasaID");

            migrationBuilder.CreateIndex(
                name: "IX_CariHareketler_CariID",
                table: "CariHareketler",
                column: "CariID");

            migrationBuilder.CreateIndex(
                name: "IX_FaturaDetaylari_BirimID",
                table: "FaturaDetaylari",
                column: "BirimID");

            migrationBuilder.CreateIndex(
                name: "IX_FaturaDetaylari_FaturaID",
                table: "FaturaDetaylari",
                column: "FaturaID");

            migrationBuilder.CreateIndex(
                name: "IX_FaturaDetaylari_UrunID",
                table: "FaturaDetaylari",
                column: "UrunID");

            migrationBuilder.CreateIndex(
                name: "IX_Faturalar_CariID",
                table: "Faturalar",
                column: "CariID");

            migrationBuilder.CreateIndex(
                name: "IX_Faturalar_FaturaTuruID",
                table: "Faturalar",
                column: "FaturaTuruID");

            migrationBuilder.CreateIndex(
                name: "IX_Faturalar_OdemeTuruID",
                table: "Faturalar",
                column: "OdemeTuruID");

            migrationBuilder.CreateIndex(
                name: "IX_FaturaOdemeleri_FaturaID",
                table: "FaturaOdemeleri",
                column: "FaturaID");

            migrationBuilder.CreateIndex(
                name: "IX_IrsaliyeDetaylari_BirimID",
                table: "IrsaliyeDetaylari",
                column: "BirimID");

            migrationBuilder.CreateIndex(
                name: "IX_IrsaliyeDetaylari_IrsaliyeID",
                table: "IrsaliyeDetaylari",
                column: "IrsaliyeID");

            migrationBuilder.CreateIndex(
                name: "IX_IrsaliyeDetaylari_UrunID",
                table: "IrsaliyeDetaylari",
                column: "UrunID");

            migrationBuilder.CreateIndex(
                name: "IX_Irsaliyeler_CariID",
                table: "Irsaliyeler",
                column: "CariID");

            migrationBuilder.CreateIndex(
                name: "IX_Irsaliyeler_FaturaID",
                table: "Irsaliyeler",
                column: "FaturaID");

            migrationBuilder.CreateIndex(
                name: "IX_Irsaliyeler_IrsaliyeTuruID",
                table: "Irsaliyeler",
                column: "IrsaliyeTuruID");

            migrationBuilder.CreateIndex(
                name: "IX_KasaHareketleri_CariID",
                table: "KasaHareketleri",
                column: "CariID");

            migrationBuilder.CreateIndex(
                name: "IX_KasaHareketleri_HedefBankaID",
                table: "KasaHareketleri",
                column: "HedefBankaID");

            migrationBuilder.CreateIndex(
                name: "IX_KasaHareketleri_HedefKasaID",
                table: "KasaHareketleri",
                column: "HedefKasaID");

            migrationBuilder.CreateIndex(
                name: "IX_KasaHareketleri_KasaID",
                table: "KasaHareketleri",
                column: "KasaID");

            migrationBuilder.CreateIndex(
                name: "IX_KasaHareketleri_KaynakBankaID",
                table: "KasaHareketleri",
                column: "KaynakBankaID");

            migrationBuilder.CreateIndex(
                name: "IX_KurDegeri_ParaBirimiID",
                table: "KurDegeri",
                column: "ParaBirimiID");

            migrationBuilder.CreateIndex(
                name: "IX_ParaBirimiIliski_HedefParaBirimiID",
                table: "ParaBirimiIliski",
                column: "HedefParaBirimiID");

            migrationBuilder.CreateIndex(
                name: "IX_ParaBirimiIliski_KaynakParaBirimiID",
                table: "ParaBirimiIliski",
                column: "KaynakParaBirimiID");

            migrationBuilder.CreateIndex(
                name: "IX_StokFifo_GirisTarihi",
                table: "StokFifo",
                column: "GirisTarihi");

            migrationBuilder.CreateIndex(
                name: "IX_StokFifo_Referans",
                table: "StokFifo",
                columns: new[] { "ReferansID", "ReferansTuru" });

            migrationBuilder.CreateIndex(
                name: "IX_StokFifo_StokSorgu",
                table: "StokFifo",
                columns: new[] { "UrunID", "KalanMiktar", "Aktif", "SoftDelete", "Iptal" });

            migrationBuilder.CreateIndex(
                name: "IX_StokFifo_UrunID",
                table: "StokFifo",
                column: "UrunID");

            migrationBuilder.CreateIndex(
                name: "IX_StokHareketleri_DepoID",
                table: "StokHareketleri",
                column: "DepoID");

            migrationBuilder.CreateIndex(
                name: "IX_StokHareketleri_UrunID",
                table: "StokHareketleri",
                column: "UrunID");

            migrationBuilder.CreateIndex(
                name: "IX_UrunFiyatlari_FiyatTipiID",
                table: "UrunFiyatlari",
                column: "FiyatTipiID");

            migrationBuilder.CreateIndex(
                name: "IX_UrunFiyatlari_UrunID",
                table: "UrunFiyatlari",
                column: "UrunID");

            migrationBuilder.CreateIndex(
                name: "IX_Urunler_BirimID",
                table: "Urunler",
                column: "BirimID");

            migrationBuilder.CreateIndex(
                name: "IX_Urunler_KategoriID",
                table: "Urunler",
                column: "KategoriID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "BankaHareketleri");

            migrationBuilder.DropTable(
                name: "CariHareketler");

            migrationBuilder.DropTable(
                name: "DovizKurlari");

            migrationBuilder.DropTable(
                name: "FaturaDetaylari");

            migrationBuilder.DropTable(
                name: "FaturaOdemeleri");

            migrationBuilder.DropTable(
                name: "IrsaliyeDetaylari");

            migrationBuilder.DropTable(
                name: "KasaHareketleri");

            migrationBuilder.DropTable(
                name: "KurDegeri");

            migrationBuilder.DropTable(
                name: "ParaBirimiIliski");

            migrationBuilder.DropTable(
                name: "SistemAyarlari");

            migrationBuilder.DropTable(
                name: "SistemLoglar");

            migrationBuilder.DropTable(
                name: "StokFifo");

            migrationBuilder.DropTable(
                name: "StokHareketleri");

            migrationBuilder.DropTable(
                name: "UrunFiyatlari");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Irsaliyeler");

            migrationBuilder.DropTable(
                name: "Bankalar");

            migrationBuilder.DropTable(
                name: "Kasalar");

            migrationBuilder.DropTable(
                name: "ParaBirimi");

            migrationBuilder.DropTable(
                name: "Depolar");

            migrationBuilder.DropTable(
                name: "FiyatTipleri");

            migrationBuilder.DropTable(
                name: "Urunler");

            migrationBuilder.DropTable(
                name: "Faturalar");

            migrationBuilder.DropTable(
                name: "IrsaliyeTurleri");

            migrationBuilder.DropTable(
                name: "Birimler");

            migrationBuilder.DropTable(
                name: "UrunKategorileri");

            migrationBuilder.DropTable(
                name: "Cariler");

            migrationBuilder.DropTable(
                name: "FaturaTurleri");

            migrationBuilder.DropTable(
                name: "OdemeTurleri");
        }
    }
}
