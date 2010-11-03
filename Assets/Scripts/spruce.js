$(document).ready(function () {
	$("#nav a").tipTip({ delay: 100 });
	$("#menubutton").overlay(); // settings button click
	chainSettingsOptions();
	bindFormSubmit();
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
	window.location = "/Tfs/Spruce/Home/NewBug/" + $("#textbox-newitem").val();
}

function chainSettingsOptions() {
	$("#settingsProject").selectChain({
		target: $("#settingsIteration"),
		url: "/Tfs/Spruce/Ajax/GetIterationsForProject/"
	});

	$("#settingsProject").selectChain({
		target: $("#settingsArea"),
		url: "/Tfs/Spruce/Ajax/GetAreasForProject/"
	});
}

//
// Primitive form error handling for now.
//
function bindFormSubmit() {
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
}


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
		url: "/Tfs/Spruce/Home/SaveSettings",
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

/* Original by Remy Sharp */
(function ($) {
	$.fn.selectChain = function (options) {
		var defaults = {
			key: "id",
			value: "label"
		};

		var settings = $.extend({}, defaults, options);

		if (!(settings.target instanceof $)) settings.target = $(settings.target);

		return this.each(function () {
			var $$ = $(this);

			$$.change(function () {
				var data = { "id" : $$.val()};

				/*
				if (typeof settings.data == 'string') {
					data = settings.data + '&' + this.name + '=' + $$.val();
				} else if (typeof settings.data == 'object') {
					data = settings.data;
					data[this.name] = $$.val();
				}*/

				settings.target.empty();
				$("#settings-savebutton").attr("disabled","disabled");

				$.ajax({
					url: settings.url,
					data: data,
					type: (settings.type || 'get'),
					dataType: 'json',
					success: function (j) {
						var options = [], i = 0, o = null;

						for (i = 0; i < j.length; i++) {
							// required to get around IE bug (http://support.microsoft.com/?scid=kb%3Ben-us%3B276228)
							o = document.createElement("OPTION");
							o.value = typeof j[i] == 'object' ? j[i][settings.key] : j[i];
							o.text = typeof j[i] == 'object' ? j[i][settings.value] : j[i];
							settings.target.get(0).options[i] = o;
						}

						// hand control back to browser for a moment
						setTimeout(function () {
							settings.target
                                .find('option:first')
                                .attr('selected', 'selected')
                                .parent('select')
                                .trigger('change');
						}, 0);

						$("#settings-savebutton").attr("disabled","");
					},
					error: function (xhr, desc, er) {
						// add whatever debug you want here.
						alert("an error occurred getting the iterations/areas");
						$("#settings-savebutton").attr("disabled","");
					}
				});
			});
		});
	};
})(jQuery);


