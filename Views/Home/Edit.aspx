<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<WorkItemSummary>" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">
<%using (new HtmlForm())
  { %>

	<input type="hidden" id="Id" value="<%=Model.Id %>" />
    <div id="itemcontainer">
        <div id="item">
            <h1><span class="workitem-id">#<%=Model.Id%></span><input type="text" name="title" value="<%=Model.Title%>" class="title" /></h1>
            <h2><!-- Assigned to -->
				Assigned to: 
				<%=Html.DropDownBoxFromList("AssignedTo",(List<string>)ViewData["Users"],Model.AssignedTo)%>

				<!-- States -->
				<%if (!Model.IsNew){ %>
				State:
				<%=Html.DropDownBoxFromList("State", (List<string>)ViewData["States"], Model.State)%>
				<%} %>

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
            <textarea name="description" class="description"><%=Model.Description%></textarea>
        </div>
        <div id="actionbar" style="text-align:right;">
            <input type="submit" value="Save" class="button" />
            <input type="reset" value="Cancel" class="button" />
        </div>
    </div>
<%} %>
</asp:Content>


