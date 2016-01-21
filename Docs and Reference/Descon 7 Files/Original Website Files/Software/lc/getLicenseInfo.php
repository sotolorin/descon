<?php
define('INCLUDE_CHECK',true);

require 'connect.php';

if(isset($_POST['ser']))
{
	$serial = $_POST['ser'];
	$result = "";

	$row = mysql_fetch_array(mysql_query("SELECT * FROM serial WHERE serial='$serial'"));

		if($row[0])
		{
			for($i = 13; $i <= 20; $i++)
			{
				$result .= $row[$i] . ",";
			}
		}
		else $result='null';
	echo $result;
}

?>