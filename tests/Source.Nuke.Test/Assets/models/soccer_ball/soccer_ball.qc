$modelname "soccer_ball/soccer_ball.mdl"

$staticprop

$bodygroup "studio"
{
	studio "soccer_ball.smd"
}

$surfaceprop "dirt"

$contents "solid"

$illumposition -0.025 0 0

$cdmaterials "models/soccer_ball/"

$cbox 0 0 0 0 0 0

$bbox -6.701 -6.831 -6.855 6.701 6.781 6.855

$definebone "static_prop" "" 0 0 0 0 0 0 0 0 0 0 0 0

$sequence "idle" {
	"soccer_ball_anims/idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 15
	loop
}

$collisionmodel "soccer_ball_physics.smd"
{
	$mass 0.1
	$inertia 1
	$damping 0
	$rotdamping 0
	$rootbone " "
}

$keyvalues
{
	prop_data 
	{
		"base"		"Plastic.Small"
	}
}
