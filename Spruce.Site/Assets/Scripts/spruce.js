$(document).ready(function ()
{
	$("#item-actionmenulink").click(function ()
	{
		if($("#item-menu").is(":visible"))
		{
			$("#item-actionmenulink img").attr("src", SPRUCE_IMAGEPATH + "/black-arrow-down.png");
			$("#item-menu").slideUp(100);
		}
		else
		{
			$("#item-actionmenulink img").attr("src", SPRUCE_IMAGEPATH + "/black-arrow-up.png");
			$("#item-menu").slideDown(100);
		}
	});

	$("#attachments-link").click(function ()
	{
		if($("#edit-attachments").is(":visible"))
		{
			$("#attachments-link img").attr("src", SPRUCE_IMAGEPATH + "/black-arrow-down.png");
			$("#edit-attachments").slideUp(100);
		}
		else
		{
			$("#attachments-linkimg").attr("src", SPRUCE_IMAGEPATH + "/black-arrow-up.png");
			$("#edit-attachments").slideDown(100);
		}
	});

	// Right side areas,iterations,filters - turn into buttons using jQuery UI.
	$("#areas input").each(function ()
	{
		$(this).button();
	});
	$("#iterations input").each(function ()
	{
		$(this).button();
	});
	$("#filters input").each(function ()
	{
		$(this).button();
	});

	// Revisions table toggle links
	$("#item-revisions a").click(function ()
	{
		var tableId = $(this).attr("title");
		$("#" + tableId).toggle();
	});
});

function toggleRevisionTable(index)
{
	$("#revision" + index).toggle();
}

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