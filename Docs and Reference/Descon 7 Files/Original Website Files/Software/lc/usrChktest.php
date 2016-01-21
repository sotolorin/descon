<?php
define('INCLUDE_CHECK',true);

require 'connect.php';

if(isset($_POST['ser']) && isset($_POST['mod']) && isset($_POST['mac']))
{
	$serial=$_POST['ser'];
	$module=$_POST['mod'];
	$macId=$_POST['mac'];
	
	$row = mysql_fetch_assoc(mysql_query("SELECT * FROM serialUsers WHERE serial='$serial' AND module='$module' AND macId='$macId'"));
	
	if($row['module'])
	{
		$result = 1;
	}
	else $result = 0;
	echo $result;


}

?>