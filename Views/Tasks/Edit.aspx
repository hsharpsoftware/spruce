﻿<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<WorkItemSummary>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
	<script type="text/javascript">
		$(document).ready(function () {
			setupEditPage();
		});
	</script>

	<%if (ViewData["Error"] != null)
	  { %>
	  <div class="errorbox">
		<h1>There was a problem saving the task</h1>
		<p>
		<b>The error TFS returned was:</b><br />
		<%=ViewData["Error"]%>
		</p>
	  </div>
	<%} %>

	<%using (Html.BeginForm(null, null, FormMethod.Post, new { id = "editform" })){ %>
	<input type="hidden" name="id" id="Id" value="<%=Model.Id %>" />

	<div id="edit-pagename" class="mid-text"><%=ViewData["PageName"] %></div>
	<div id="edit-container">
		<div id="edit-title">
			<%=Html.TextBoxFor<WorkItemSummary, string>(i => i.Title, new { tabindex = 1, title = "Enter your title",placeholder="Enter your title" })%>
		</div>

		<div class="edit-options lightbg">
			<div class="edit-field">
				<div class="label">Assigned to:</div>
				<div class="value"><%=Html.DropDownBoxFromList("AssignedTo",(List<string>)ViewData["Users"],Model.AssignedTo)%></div>
			</div>
		
			<div class="edit-field">
				<div class="label">State:</div>
				<div class="value"><%=Html.DropDownBoxFromList("State", (List<string>)ViewData["States"], Model.State)%></div>
			</div>

			<div class="edit-field">
				<div class="label">Priority:</div>
				<div class="value"><%=Html.DropDownBoxFromList("Priority", (List<string>)ViewData["Priorities"], Model.Priority.Value.ToString())%></div>
			</div>
			<br style="clear:both"/>

			<div class="edit-field">
				<div class="label">Area:</div>
				<div class="value"><%=Html.DropDownBoxForAreas("AreaPath", (List<AreaSummary>)ViewData["Areas"], Model.AreaPath)%></div>
			</div>

			<div class="edit-field">
				<div class="label">Iteration:</div>
				<div class="value"><%=Html.DropDownBoxForIterations("IterationPath", (List<IterationSummary>)ViewData["Iterations"], Model.IterationPath)%></div>
			</div>
			<br style="clear:both"/>
		
			<div class="edit-field-long">
				<div class="label">Estimated hours:</div>
				<div class="value"><%=Html.TextBoxFor<WorkItemSummary, int>(w => w.EstimatedHours, new { @class = "twochar-textbox" })%></div>
			</div>

			<div class="edit-field-long">
				<div class="label">Remaining hours:</div>
				<div class="value"><%=Html.TextBoxFor<WorkItemSummary, int>(w => w.RemainingHours, new { @class = "twochar-textbox" })%></div>
			</div>

			<div class="edit-field-long">
				<div class="label">Completed hours:</div>
				<div class="value"><%=Html.TextBoxFor<WorkItemSummary, int>(w => w.CompletedHours, new { @class = "twochar-textbox" })%></div>
			</div>
			<br style="clear:both"/>
		</div>

		<div id="edit-description">
			<%=Html.TextAreaFor<WorkItemSummary, string>(i => i.Description, new { tabindex = 2, title = "Enter a brief description. See your project template for guidance", placeholder = "Enter a brief description. See your project template for guidance" })%>
		</div>

		<div id="edit-buttons">
			<input type="submit" value="Save" />
			<input type="button" value="Cancel" onclick="history.go(-1);"  />
		</div>
	</div>
	<%} %>
</asp:Content>

