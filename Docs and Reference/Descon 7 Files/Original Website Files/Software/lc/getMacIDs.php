<?php
define('INCLUDE_CHECK',true);

require 'connect.php';

if(isset($_POST['ser']) && isset($_POST['mod']))
{
	$serial=$_POST['ser'];
	$module=$_POST['mod'];
	
	$result = mysql_query("SELECT macId FROM serialUsers WHERE serial='$serial' AND module='$module'");
	$macIDs = "";
	
	while($row = mysql_fetch_assoc($result)){
		$macIDs = $macIDs . "\n" . $row['macId'];
	}
	
	echo $macIDs;


}

?>