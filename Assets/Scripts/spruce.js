$(document).ready(function () {
	$("#nav a").tipTip({ delay: 100 });
	$("#menubutton").overlay(); // settings button click
});

function highlightNavItem(id) {
	$(id).addClass("selected");
}

function toggleNewItem() {
	$("#newitem").toggle();
	$("#newitem-textbox").toggle();
	$("#textbox-newitem").focus();
}

function addBug() {
	window.location = "/Spruce/Home/NewBug/" + $("#textbox-newitem").val();
}

//
// Primitive form error handling for now.
//
$("#editform").submit(function () {
	$("#Title").removeClass("textbox-error");
	$("#Description").removeClass("textbox-error");

	if ($("#Title").val().length < 1) {
		$("#Title").addClass("textbox-error");
		//$("#Title").expose();
		$("#Title").focus();
		return false;
	}
	if ($("#Description").val().length < 1) {
		$("#Description").addClass("textbox-error");
		//$("#Description").expose();
		$("#Description").focus();
		return false;
	}

	return true;
});


function focusTextboxes() {
	var title = $("#Title").val();
	if (title === "")
		$("#Title").focus();
	else
		$("#Description").focus();
}

// Unused, for now.
function saveSettings() {
	var dataValues = {
		project: $("#settings-project").selectedValue(),
		iteration: $("#settings-iteration").selectedValue(),
		area: $("#settings-area").selectedValue(),
		states: $("#settings-active").isChecked() + "," + $("#settings-resolved").isChecked() + "," + $("#settings-closed").isChecked()
	};

	$.ajax({
		url: "/Spruce/Home/SaveSettings",
		type: "POST",
		data: dataValues,
		dataType: "json",
		contentType: "application/json; charset=utf-8",
		success: function () {
			$("#menubutton[rel]").close();
		},
		error: function (e) {
			alert(e);
			alert("Unable to save the settings");
		}
	});
}

