<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<List<WorkItemSummary>>" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<script type="text/javascript" charset="utf-8">
$(document).ready(function ()
{
	var currentAction = "<%=ViewContext.Controller.ValueProvider.GetValue("action").RawValue%>".toLowerCase();
	highlightNavItem("#navitem-"+currentAction);
});
</script>

<div id="taskscontainer">
	<%foreach (WorkItemSummary item in Model){ %>
	<div id="task">
		<h1>
			<span class="workitem-id">#<%=item.Id%></span><br />
			<a href="/Tfs/Spruce/Home/View/<%=item.Id%>"><%=item.Title %></a>
		</h1>
		<p>
		Assigned to <%=item.AssignedTo %> (<%=item.State %>)<br />
		<span style="font-size:0.6em"><%=item.CreatedDate.ToShortDateString()%></div>
		</p>
		
	</div>
	<%} %>
</div>
</asp:Content>


