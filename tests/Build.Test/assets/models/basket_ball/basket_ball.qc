$modelname "basket_ball/basket_ball.mdl"

$bodygroup "studio"
{
	studio "basket_ball.smd"
}

$surfaceprop "dirt"

$contents "solid"

$illumposition -0.076 -0.282 16.691

$cdmaterials "models/basket_ball/"

$cbox 0 0 0 0 0 0

$bbox -7.085 -7.101 -7.062 7.101 7.084 7.063

$definebone "smdimport" "" 0 0 0 0 0 0 0 0 0 0 0 0

$sequence "idle" {
	"basket_ball_anims/idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 30
}

$collisionmodel "basket_ball_physics.smd"
{
	$mass 10
	$inertia 0.2
	$damping 0
	$rotdamping 0
	$rootbone " "
}

$keyvalues
{
	prop_data 
	{
		"base"		"Wooden.Base"
	}
}
