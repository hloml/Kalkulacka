<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="FareCalculator.aspx.cs" Inherits="WebPovedCalculator.FareCalculator" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <meta charset="utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta name="viewport" content="width=device-width, initial-scale=1" />
    <!-- The above 3 meta tags *must* come first in the head; any other head content must come *after* these tags -->
    <!-- Bootstrap -->
    <link href="res/bootstrap-3.3.4-dist/css/bootstrap.min.css" rel="stylesheet" />
    <!-- HTML5 shim and Respond.js for IE8 support of HTML5 elements and media queries -->
    <!-- WARNING: Respond.js doesn't work if you view the page via file:// -->
    <!--[if lt IE 9]>
      <script src="https://oss.maxcdn.com/html5shiv/3.7.2/html5shiv.min.js"></script>
      <script src="https://oss.maxcdn.com/respond/1.4.2/respond.min.js"></script>
    <![endif]-->
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div class="container">
        <!-- Calculator head -->
        <div class="row">
            <div class="col-md-6">
                <h1>Kalkulátor ceny jízdného</h1>
            </div>
        </div>
        <div class="row">
            <div class="col-md-2">
                Kategorie
            </div>
            <div class="col-md-3">
                <select name="category" size="1">
                    <option value="0">Sleva Žádná</option>
                    <option value="1">Sleva Malá</option>
                    <option value="2">Sleva Velká</option>
                    <option value="3">Sleva 100%</option>
                </select>
            </div>
        </div>
        <div class="row">
            <div class="col-md-2">
                Doba předplatného:
            </div>
            <div class="col-md-3">
                Od
                <input type="text" size="10" name="start_date" value="dneska" />
                Do
                <input type="text" size="10" name="end_date" value="zejtra" />
            </div>
        </div>
        <!-- Zones -->
        <div class="row">
            <div class="col-md-6">
                <h2>Zóny</h2>
            </div>
        </div>
        <div class="row">
            <div class="col-md-2">
                Plzeň:
            </div>
            <div class="col-md-3">
                <input type="checkbox" name="pilsen" value="yes" />
            </div>
        </div>
        <div class="row">
            <div class="col-md-2">
                Počet Vnějších zón:
            </div>
            <div class="col-md-3">
                <select name="zones_sum" size="1">
                    <option value="0">Žádná</option>
                    <option value="1">1</option>
                    <option value="2">2</option>
                    <option value="3">3</option>
                    <option value="4">4</option>
                    <option value="5">5</option>
                    <option value="6">6</option>
                    <option value="7">7</option>
                </select>
            </div>
        </div>
        <div class="row">
            <div class="col-md-2">
            </div>
            <div class="col-md-4">
                <input type="submit" class="btn btn-primary" value="Spočítat cenu" />
            </div>
        </div>
    </div>
    </form>
<!-- jQuery (necessary for Bootstrap's JavaScript plugins) -->
<script src="https://ajax.googleapis.com/ajax/libs/jquery/1.11.2/jquery.min.js"></script>
<!-- Include all compiled plugins (below), or include individual files as needed -->
<script src="res/bootstrap-3.3.4-dist/js/bootstrap.min.js"></script>
</body>
</html>
