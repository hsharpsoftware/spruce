<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<WorkItemSummary>" %>
<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" runat="server">

    <div id="itemcontainer">
        <div id="item">
            <h1><span class="workitem-id">#<%=Model.Id%></span><br /><%=Model.Title%></h1>
            <h2><%=Model.State%> | Assigned to <%=Model.AssignedTo%></h2>
            <p>
				<%=Model.Description.Replace("\n","<br/>")%>
            </p>
        </div>
        <div id="favouritebar">
            <div id="bottom-info"><%=Model.CreatedDate.ToString()%>&nbsp;|&nbsp;<b>Area:</b> <%=Model.Area%>&nbsp;|&nbsp;<b>Iteration:</b> <%=Model.Iteration%></div>
            <div style="float:right;"><a href="javascript:;"><img src="/Tfs/Spruce/Assets/Images/star_on.png" border="0" /></a></div>
        </div>
        <br style="clear:both"/>
        <div id="actionbar">
            <a href="/Tfs/Spruce/Home/Edit/<%=Model.Id %>">Edit</a>&nbsp;|&nbsp;
            <a href="/Tfs/Spruce/Home/Resolve/<%=Model.Id %>">Resolve</a>&nbsp;|&nbsp;
            <a href="/Tfs/Spruce/Home/Close/<%=Model.Id %>">Close</a>
        </div>
    </div>

</asp:Content>


