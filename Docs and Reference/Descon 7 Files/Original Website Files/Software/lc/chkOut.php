<?php
define('INCLUDE_CHECK',true);

require 'connect.php';

if(isset($_POST['ser']) && isset($_POST['mod'])&& isset($_POST['mac']))
{
	$serial=$_POST['ser'];
	$module=$_POST['mod'];
	$macId=$_POST['mac'];
	$user=$_POST['usr'];
	$accessed=$_POST['acsd'];
	$version=$_POST['ver'];
	$moduleUsed=$module . "Used";
	
	$row = mysql_fetch_assoc(mysql_query("SELECT $module FROM serial WHERE serial='$serial'"));
	if($row[$module])
	{
		$numberTotal=$row[$module];
	}
	else
	{
		$numberTotal=0;
	}
	
	$row = mysql_fetch_assoc(mysql_query("SELECT $moduleUsed FROM serial WHERE serial='$serial'"));
	if($row[$moduleUsed])
	{
		$numberUsed=$row[$moduleUsed];
	}
	else
	{
		$numberUsed=0;
	}
	
	if($numberUsed < $numberTotal)
	{
		$numUsedPlusOne=$numberUsed+1;
		$res = mysql_query("UPDATE serial SET $moduleUsed='$numUsedPlusOne' WHERE serial='$serial'") or die("Unable to run query1");
		$row = mysql_fetch_assoc(mysql_query("SELECT macId FROM serialUsers WHERE serial='$serial' AND macId='$macId' AND module='$module'"));
		if($row['macId'])
		{
			$numberOfUsers=1;
		}
		else 
		{
			$numberOfUsers=0;
		}
		
		if($numberOfUsers==0)
		{
			$res = mysql_query("INSERT INTO serialUsers (serial, user, macId, accessed, module, version) VALUES ('$serial','$user','$macId','$accessed','$module','$version')") or die("Unable to run query2");
			if($res)
			{
				$result=1;
			}
		}
		else 
		{
			$result=3;
		}
	}
	else 
	{
		$result=4;
	}
	
	echo $result;


}

?>