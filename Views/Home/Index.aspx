<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<List<WorkItemSummary>>" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<script type="text/javascript" charset="utf-8">
$(document).ready(function ()
{
	$("#workitems").dataTable();

	var currentAction = "<%=ViewContext.Controller.ValueProvider.GetValue("action").RawValue%>".toLowerCase();
	highlightNavItem("#navitem-"+currentAction);
});
</script>

<!--
Active,closed,resolved
Iterations
Areas
Assigned to
Resolved by

History
Links
Quick add
Favourites
-->

<table id="workitems">
	<thead>
		<tr>
            <th style="width:5%">ID</th>
            <th style="width:65%">Title</th>
            <th style="width:15%">Assigned to</th>
            <th style="width:15%">Status</th>
		</tr>
	</thead>
	<tbody>
	<%foreach (WorkItemSummary item in Model){ %>
		<tr>
			<td><a href="/Spruce/Home/View/<%=item.Id%>"><%=item.Id%></a></td>
			<td><%=item.Title %></td>
			<td><%=item.AssignedTo %></td>
			<td><%=item.State %></td>
		</tr>
	<%} %>
	</tbody>
</table>
</asp:Content>


