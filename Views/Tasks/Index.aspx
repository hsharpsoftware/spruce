<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<List<WorkItemSummary>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
	<div id="tasks-container">
		<%foreach (WorkItemSummary item in Model){ %>
		<div class="task">
			<h1>
				<span class="workitem-id">#<%=item.Id%></span><br />
				<%=Html.ActionLink(item.Title,"View",new { id = item.Id}) %>
			</h1>
			<p>
			Assigned to <%=item.AssignedTo %> (<%=item.State %>)<br />
			<span style="font-size:0.6em"><%=item.CreatedDate.ToShortDateString()%></span>
			</p>
		
		</div>
		<%} %>
	</div>
</asp:Content>


