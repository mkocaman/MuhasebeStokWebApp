<%@ Page Title="Login - MaOaKApp" Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="MaOaKApp.Login" %>
<!DOCTYPE html>
<html lang="en">
<head runat="server">
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no" />
    <title>Login - MaOaKApp</title>
    <!-- Stil dosyaları -->
    <link rel="stylesheet" type="text/css" href="../assets/css/fontawesome.css" />
    <link rel="stylesheet" type="text/css" href="../assets/css/icofont.css" />
    <link rel="stylesheet" type="text/css" href="../assets/css/themify.css" />
    <link rel="stylesheet" type="text/css" href="../assets/css/bootstrap.css" />
    <link rel="stylesheet" type="text/css" href="../assets/css/style.css" />
    <link id="color" rel="stylesheet" href="../assets/css/color-1.css" media="screen" />
    <link rel="stylesheet" type="text/css" href="../assets/css/responsive.css" />
</head>
<body>
    <form id="form1" runat="server">
        <section>
            <div class="container-fluid">
                <div class="row">
                    <div class="col-xl-7 d-none d-xl-block">
                        <img class="bg-img-cover bg-center" src="../assets/images/login/2.jpg" alt="loginpage" />
                    </div>
                    <div class="col-xl-5 p-0">
                        <div class="login-card">
                            <div class="theme-form login-form">
                                <h4>Giriş Yap</h4>
                                <h6>Hesabınıza giriş yapın</h6>
                                <div class="form-group">
                                    <label for="txtUsername">Kullanıcı Adı</label>
                                    <div class="input-group">
                                        <span class="input-group-text"><i class="icon-user"></i></span>
                                        <asp:TextBox ID="txtUsername" runat="server" CssClass="form-control" placeholder="Kullanıcı Adı"></asp:TextBox>
                                    </div>
                                </div>
                                <div class="form-group">
                                    <label for="txtPassword">Şifre</label>
                                    <div class="input-group">
                                        <span class="input-group-text"><i class="icon-lock"></i></span>
                                        <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="form-control" placeholder="Şifre"></asp:TextBox>
                                    </div>
                                </div>
                                <asp:Label ID="lblMessage" runat="server" CssClass="text-danger" Visible="false"></asp:Label>
                                <div class="form-group">
                                    <asp:Button ID="btnLogin" runat="server" Text="Giriş Yap" CssClass="btn btn-primary btn-block" OnClick="btnLogin_Click" />
                                </div>
                                <div class="form-group">
                                    <p class="text-center">Hesabınız yok mu? <a href="sign-up.html">Hesap Oluştur</a></p>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </section>
    </form>
    
    <!-- Script dosyaları -->
    <script src="../assets/js/jquery-3.5.1.min.js"></script>
    <script src="../assets/js/bootstrap/popper.min.js"></script>
    <script src="../assets/js/bootstrap/bootstrap.min.js"></script>
    <script src="../assets/js/icons/feather-icon/feather.min.js"></script>
    <script src="../assets/js/icons/feather-icon/feather-icon.js"></script>
    <script src="../assets/js/script.js"></script>
</body>
</html>