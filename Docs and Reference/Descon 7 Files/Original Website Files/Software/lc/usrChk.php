<?php
define('INCLUDE_CHECK',true);

require 'connect.php';

if(isset($_POST['ser']) && isset($_POST['mod']) && isset($_POST['mac']))
{
	$serial=$_POST['ser'];
	$module=$_POST['mod'];
	$macId=$_POST['mac'];
	
	$macIDs = explode("\n", $macId);
	$count = count($macIDs);
	
	
	
	for($i = 0; $i < $count; $i++){
	
		$row = mysql_fetch_assoc(mysql_query("SELECT * FROM serialUsers WHERE serial='$serial' AND module='$module' AND macId='$macIDs[$i]'"));
		if($row['module'])
		{
			$result = 1;
			break;
		}
		else $result = 0;
	}
		
	echo $result;


}

?>