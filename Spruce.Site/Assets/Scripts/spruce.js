/// <reference path="jquery-1.4.1-vsdoc.js" />
/// <reference path="jquery.form-extensions.min.js" />

$(document).ready(function ()
{
	// Projects, areas, iterations drop downs in the header
	$("#header-first select").change(function ()
	{
		$(this).parent().parent().submit();
	});

	// Table filters row - return key submits the form
	$("#list-table #title").keypress(filterFormKeyPress);
	$("#list-table #assignedTo").keypress(filterFormKeyPress);
	$("#list-table #startDate").keypress(filterFormKeyPress);
	$("#list-table #endDate").keypress(filterFormKeyPress);
	$("#list-table #status").change(function ()
	{
		$("#filterform").submit();
	});

	//
	// Close/resolve drop out and attachments drop out.
	//
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

	// Pagesize drop down submits the form when the value changes
	$("#pageSize").change(function ()
	{
		$(this).parent().submit();
	});

	// Revisions table toggle links
	$("#item-revisions a").click(function ()
	{
		var tableId = $(this).attr("title");
		$("#" + tableId).toggle();
	});

	// Remove the domain portion of all logins, e.g. Company.us\Fred Jones becomes Fred Jones
	$("td>div,#header>div.right,.smaller").each(function (index, value)
	{
		var html = $(this).html();

		if(html.indexOf("\\") != -1)
		{
			html = html.replace(/\(.*?\.*?\)/g, "");
			$(this).html(html);
		}
	});

	// Format XML inside <pre> tags so that it displays
	$("pre").each(function (index)
	{
		var current = $(this);
		var html = current.html();
		html = html.replace(/</g, "&lt;").replace(/>/g, "&gt;");
		current.html(html);
	});

	// Areas/iterations/filter checkbox/radioboxes automatically postback
	$("#areas input,#iterations input,#filters input").change(function ()
	{
		$("form").submit();
	});

	// Set all .timeago classes to friendly dates.
	$(".timeago").each(function ()
	{
		$(this).timeago();
	});

	// Tooltips for the filters
	$(".help").tooltip(
	{
		bodyHandler: function ()
		{
			return $(this).attr("help");
		},
		showURL: false
	});

	// Show/hide the search help box on the search page
	$("#search-help a").click(function()
	{
		$("#search-help div").toggle();
	});
});

function filterFormKeyPress(e)
{
	if(e.which == 13)
	{
		$("#filterform").submit();
		e.preventDefault();
		return false;
	}
}

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

/*
	$("#State").change(function () {
		alert($("#State").selectedValue());
		$.get("../GetReasonsForState/" + $("#State").selectedValue(), function (data) {
			alert(data);
		});
	});
*/

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