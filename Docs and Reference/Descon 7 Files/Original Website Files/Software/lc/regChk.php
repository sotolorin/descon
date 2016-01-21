<?php
define('INCLUDE_CHECK',true);

require 'connect.php';

if(isset($_POST['ser']) && isset($_POST['reg']))
{
	$row = mysql_fetch_assoc(mysql_query("SELECT company FROM serial WHERE serial='{$_POST['ser']}' AND RegCode='{$_POST['reg']}'"));

		if($row['company'])
		{
			$result=$row['company'];
		}
		else $result='null';
	
	
	echo $result;


}

?>