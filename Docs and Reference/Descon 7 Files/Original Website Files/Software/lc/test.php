<?php
define('INCLUDE_CHECK',true);

require 'connect.php';

if(isset($_POST['user']) && isset($_POST['pass']))
{
	$row = mysql_fetch_assoc(mysql_query("SELECT company FROM serial WHERE serial='{$_POST['user']}' AND RegCode='{$_POST['pass']}'"));

		if($row['company'])
		{
			$result=$row['company'];
		}
		else $result='null';
	
	
	echo $result;


}

?>