$(document).ready(function () {
	setupAllPages();	
});

function setupAllPages() {
	bindCollapsablePanels();
}

function setupListPage() {
	$("#workitems-table").dataTable();

	// Add return key support for the new box
	$("#newitem-textbox").bind("keypress", function(e)
	{
		var code = (e.keyCode ? e.keyCode : e.which);
		 if(code == 13) { 
			addBug();
		 }
	});
}
function bindCollapsablePanels() {
	$(".toggleitem a").click(function () {
		var img = $(this).find("img");
		var src = img.attr("src");

		if (src.indexOf("minus") != -1) {
			// Expanded
			src = src.replace("minus", "plus");
		}
		else {
			// Collapsed
			src = src.replace("plus", "minus");
		}

		img.attr("src", src);

		$(this).find("img").attr("src");
		$(this).parent().parent().next().toggle();
	});
}

function setupEditPage() {
	var title = $("#Title").val();
	if (title === "")
		$("#Title").focus();
	else
		$("#Description").focus();

	bindFormSubmit();
}

function toggleNewItem() {
	$("#newitem").toggle();
	$("#newitem-textbox").toggle();
	$("#textbox-newitem").focus();
}

function addBug() {
	window.location = "./New/?id=" + $("#textbox-newitem").val();
}

function addTask()
{
	window.location = "./New/?id=" + $("#textbox-newitem").val();
}

//
// Primitive form error handling for now.
//
function bindFormSubmit() {
	$("#editform").submit(function () {
		$("#Title").removeClass("edit-error");
		$("#Description").removeClass("edit-error");

		if ($("#Title").val().length < 1) {
			$("#Title").addClass("edit-error");
			//$("#Title").expose();
			$("#Title").focus();
			return false;
		}
		if ($("#Description").val().length < 1) {
			$("#Description").addClass("edit-error");
			//$("#Description").expose();
			$("#Description").focus();
			return false;
		}

		return true;
	});
}