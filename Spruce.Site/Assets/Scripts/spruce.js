$(document).ready(function ()
{
	$("#togglemenu").click(function ()
	{
		if($("#popoutmenu").is(":visible"))
		{
			$("#togglemenu img").attr("src", SPRUCE_IMAGEPATH +"/arrow-down.png");
			$("#popoutmenu").slideUp();
		}
		else
		{
			$("#togglemenu img").attr("src", SPRUCE_IMAGEPATH +"/arrow-up.png");
			$("#popoutmenu").slideDown();
		}
	});

	$("#actionmenu-link").click(function ()
	{
		if($("#actionmenu").is(":visible"))
		{
			$("#actionmenu-link img").attr("src", SPRUCE_IMAGEPATH + "/black-arrow-down.png");
			$("#actionmenu").slideUp(100);
		}
		else
		{
			$("#actionmenu-link img").attr("src", SPRUCE_IMAGEPATH + "/black-arrow-up.png");
			$("#actionmenu").slideDown(100);
		}
	});
});

//
// Primitive form error handling (for now).
//
function setupEditPage()
{
	var title = $("#Title").val();
	if(title === "")
		$("#Title").focus();

	bindFormSubmit();
}

function bindFormSubmit() {
	$("#editform").submit(function () {
		$("#Title").removeClass("edit-error");

		if ($("#Title").val().length < 1) {
			$("#Title").addClass("edit-error");
			$("#Title").focus();
			return false;
		}
		return true;
	});
}