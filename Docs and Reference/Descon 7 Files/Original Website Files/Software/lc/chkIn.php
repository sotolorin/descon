<?php
define('INCLUDE_CHECK',true);

require 'connect.php';

if(isset($_POST['ser']) && isset($_POST['mod'])&& isset($_POST['mac']))
{
	$serial=$_POST['ser'];
	$module=$_POST['mod'];
	$macId=$_POST['mac'];
	$moduleUsed=$module . "Used";
	
	$macIDs = explode("\n", $macId);
	$count = count($macIDs);
	
	$row = mysql_fetch_assoc(mysql_query("SELECT $module FROM serial WHERE serial='$serial'"));
	if($row[$module])
	{
		$numberTotal=$row[$module];
	}
	else $numberTotal=0;
	
	$row = mysql_fetch_assoc(mysql_query("SELECT $moduleUsed FROM serial WHERE serial='$serial'"));
	if($row[$moduleUsed])
	{
		$numberUsed=$row[$moduleUsed];
	}
	else $numberUsed=0;
	
	if($numberUsed>0)
	{
		$nthID=0;
		$numUsedMinusOne=$numberUsed-1;
		$res = mysql_query("UPDATE serial SET $moduleUsed='$numUsedMinusOne' WHERE serial='$serial'") or die("Unable to run query");
		
		for($i = 0; $i < $count; $i++){
			$row = mysql_fetch_assoc(mysql_query("SELECT * FROM serialUsers WHERE serial='$serial' AND macId LIKE '%$macIDs[$i]%' AND module='$module'"));
			if($row['module'])
			{
				$numberOfUsers=1;
				$nthID=$i;
				break;
			}
			else $numberOfUsers=0;
		}
		if($numberOfUsers>0)
		{
			$res = mysql_query("DELETE FROM serialUsers WHERE serial='$serial' AND macId LIKE '%$macIDs[$nthID]%' AND module='$module'") or die("Unable to run query");
			if($res)
			{
				$result=1;
			}
		}
		else $result=3;
	}
	else $result=4;
	
	echo $result;


}

?>