<?php
define('INCLUDE_CHECK',true);

require 'connect.php';

if(isset($_POST['ser']) && isset($_POST['ver']))
{
	$serial=$_POST['ser'];
	$version=$_POST['ver'];
	
	$row = mysql_fetch_assoc(mysql_query("SELECT $version FROM serial WHERE serial='$serial'"));
	if($row[$version])
	{
		$currentVersion=$row[$version];
	}
	else $currentVersion='unknown';
	
	
	echo $currentVersion;


}

?>