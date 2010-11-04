<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<WorkItemSummary>" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

<script type="text/javascript">
	$(document).ready(function () {
		focusTextboxes();
	});
</script>
<%using (Html.BeginForm(null, null, FormMethod.Post, new { id = "editform" })){ %>
<input type="hidden" id="Id" value="<%=Model.Id %>" />

<h2 style="text-align:right"><%=ViewData["Message"] %></h2>
<div id="itemcontainer">
	<div id="item">
		<h1>
			<%if (Model.Id > 0){ %>
			<span class="workitem-id">#<%=Model.Id%></span><br />
			<%} %>
			<%=Html.TextBoxFor<WorkItemSummary, string>(i => i.Title, new { @class = "textbox-title defaulttext", title = "Enter your title",placeholder="Enter your title" })%>
		</h1>
		<h2><!-- Assigned to -->
			Assigned to: 
			<%=Html.DropDownBoxFromList("AssignedTo",(List<string>)ViewData["Users"],Model.AssignedTo)%>

			<!-- States -->
			State:
			<%=Html.DropDownBoxFromList("State", (List<string>)ViewData["States"], Model.State)%>
				
			<!-- Priorities -->
			Priority:
			<%=Html.DropDownBoxFromList("Priority", (List<string>)ViewData["Priorities"], Model.Priority.Value.ToString())%>

			<br />

			<!-- Area -->
			Area:
			<%=Html.DropDownBoxForAreas("Area", (List<AreaSummary>)ViewData["Areas"], Model.Area)%>

			<!-- Iteration -->
			Iteration:
			<%=Html.DropDownBoxForIterations("Iteration", (List<IterationSummary>)ViewData["Iterations"], Model.Iteration)%>
		</h2>
		<%=Html.TextAreaFor<WorkItemSummary, string>(i => i.Description, new { @class = "textbox-description defaulttext", title = "Enter a brief description. See your project template for guidance", placeholder = "Enter a brief description. See your project template for guidance" })%>
	</div>
	<div id="actionbar" style="text-align:right;">
		<input type="submit" value="Save" class="button" />
		<input type="button" value="Cancel" class="button" onclick="history.go(-1);"  />
	</div>
</div>
<%} %>
</asp:Content>


