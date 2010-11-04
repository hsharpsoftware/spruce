<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<List<WorkItemSummary>>" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<script type="text/javascript" charset="utf-8">
$(document).ready(function ()
{
	$("#workitems-table").dataTable();

	var currentAction = "<%=ViewContext.Controller.ValueProvider.GetValue("action").RawValue%>".toLowerCase();
	highlightNavItem("#navitem-"+currentAction);

	// Add return key support for the new box
	$("#newitem-textbox").bind("keypress", function(e)
	{
		var code = (e.keyCode ? e.keyCode : e.which);
		 if(code == 13) { 
			addBug();
		 }
	});
});
</script>

<table id="workitems-table">
	<thead>
		<tr>
			<th style="width:5%">ID</th>
			<th style="width:60%">Title</th>
			<th style="width:20%">Assigned to</th>
			<th style="width:15%">Status</th>
		</tr>
	</thead>
	<tbody>
	<%foreach (WorkItemSummary item in Model){ %>
		<tr>
			<td><a href="/Tfs/Spruce/Home/View/<%=item.Id%>"><%=item.Id%></a></td>
			<td><%=item.Title %></td>
			<td><%=item.AssignedTo %></td>
			<td><%=item.State %></td>
		</tr>
	<%} %>
	</tbody>
</table>
<br style="clear:both" />

<div id="newitem-container">
    <div id="newitem">&gt;&nbsp;<a href="javascript:toggleNewItem()">New bug</a></div>
    <div id="newitem-textbox">
        <input type="text" id="textbox-newitem" style="width:350px;" />
        <input type="button" value="Save" onclick="addBug()" />
        <input type="button" value="Cancel" onclick="toggleNewItem()" />
    </div>
</div>

<br />
<div id="filterdetails">
<%
if ((bool)ViewData["HasIterationData"] || (bool)ViewData["HasAreaData"])
{ 
%>	
	<%
		if ((bool)ViewData["HasIterationData"])
		{ 
	%>	
		<div style="float:left;margin-right:10px"><b>Iteration:&nbsp;</b><%=ViewData["IterationPath"]%></div>		
	<%
		} 
	%>
	<%
		if ((bool)ViewData["HasAreaData"])
		{ 
	%>	
		<div style="float:left;"><b>Area:&nbsp;</b><%=ViewData["AreaPath"]%></div>		
	<%
		}
	%>
<br style="clear:both"/>
<%}%>

	<%=Html.HtmlForListFilters("All bugs", "All bugs for the current project", "Index")%>
	<%=Html.HtmlForListFilters("Active bugs","All active bugs for the current project","Active")%>
	<%=Html.HtmlForListFilters("Closed bugs","All closed bugs for the current project","Closed")%>
	<%=Html.HtmlForListFilters("Tasks","All tasks for the current project","AllTasks")%>
	<%=Html.HtmlForListFilters("All items", "Every type of work item for the current project", "AllItems")%>
</div>

</asp:Content>


