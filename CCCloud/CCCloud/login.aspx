<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="login.aspx.cs" Inherits="CCCloud.login1" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <meta charset="utf-8"/>
<title>登录</title>
<meta name="author" content="DeathGhost" />
    <link href="CSS/login1.css" rel="stylesheet" />
<style>
body{height:100%;background:#16a085;overflow:hidden;}
canvas{z-index:-1;position:absolute;}
</style>
    <script src="JavaScript/jquery-3.3.1.min.js"></script>
    <script src="JavaScript/verificationNumbers.js"></script>
    <script src="JavaScript/Particleground.js"></script>
<script>
    $(document).ready(function () {
        //粒子背景特效
        $('body').particleground({
            dotColor: '#5cbdaa',
            lineColor: '#5cbdaa'
        });
        //验证码
       // createCode();
    });
</script>

</head>
<body>
   <dl class="admin_login">
    <dt>
        <strong>常理工自动驾驶管理系统</strong>
        <em>CIT Automatic Drive Management System</em>
    </dt>
       <form class="form" runat="server">
           <dd class="user_icon">
             <asp:TextBox ID="UsernameTextbox" runat="server" placeholder="账号" class="login_txtbx"></asp:TextBox>
           </dd>
           <dd class="pwd_icon">
            <asp:TextBox ID="PasswordTextbox" runat="server" placeholder="密码" class="login_txtbx"></asp:TextBox>
           </dd>
           <dd class="val_icon">
                
               <asp:Button ID="keren" runat="server" Text="访客模式" class="submit_btn" OnClick="ok"/>
           </dd>
          <dd>
            <asp:Button ID="submit" runat="server" Text="立即登陆" class="submit_btn" OnClick="submit_Click"/>
          </dd>
           
       </form>
 <dd>
  <p>(c) 2018-2019 姚锋 版权所有</p>
 </dd>
</dl>

</body>
</html>
