<?php
define('INCLUDE_CHECK',true);

require 'connect.php';

if(isset($_POST['ser']) && isset($_POST['reg']))
{
	$row = mysql_fetch_assoc(mysql_query("SELECT * FROM serial WHERE serial='{$_POST['ser']}' AND RegCode='{$_POST['reg']}'"));

		$result=$row;
	
	
	echo $row['company'] . "?" . $row['serial'] . "?" . $row['type'] . "?" . $row['DesconWinASD'] . "?" . $row['DesconWinLRFD'] . "?" . $row['DesconWinASDM'] . "?" . $row['DesconWinLRFDM'] . "?" . $row['DesconBraceASD'] . "?" . $row['DesconBraceLRFD'] . "?" . $row['DesconBraceASDM'] . "?" . $row['DesconBraceLRFDM'] . "?" . $row['EffectiveDate'] . "?" . $row['RegCode'] . "?" . $row['DesconWinASDUsed'] . "?" . $row['DesconWinLRFDUsed'] . "?" . $row['DesconWinASDMUsed'] . "?" . $row['DesconWinLRFDMUsed'] . "?" . $row['DesconBraceASDUsed'] . "?" . $row['DesconBraceLRFDUsed'] . "?" . $row['DesconBraceASDMUsed'] . "?" . $row['DesconBraceLRFDMUsed'] . "?" . $row['DWversion'] . "?" . $row['DBversion'];


}

?>