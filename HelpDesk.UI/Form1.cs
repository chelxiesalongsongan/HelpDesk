using HelpDesk.BLL;
using HelpDesk.DAL;
using HelpDesk.Model;
using HelpDesk.DTO;

namespace HelpDesk.UI
{
    public partial class Form1 : Form
    {
        private readonly ITicketService _ticketService;
        private readonly ITicketCategoryRepository _ticketCategoryRepository;
        private readonly IEmployeeRepository _employeeRepository;

        public Form1(
            ITicketService ticketService,
            ITicketCategoryRepository ticketCategoryRepository,
            IEmployeeRepository employeeRepository)
        {
            InitializeComponent();
            _ticketService = ticketService;
            _ticketCategoryRepository = ticketCategoryRepository;
            _employeeRepository = employeeRepository;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadDefaultValues();
            LoadTickets();
        }

        private void LoadDefaultValues()
        {
            cmbCategory.DataSource = _ticketCategoryRepository.GetAll();
            cmbCategory.DisplayMember = "Name";
            cmbCategory.ValueMember = "Id";

            cmbAssignedTo.DataSource = _employeeRepository.GetAll();
            cmbAssignedTo.DisplayMember = "FullName";
            cmbAssignedTo.ValueMember = "Id";

            cmbStatus.Items.AddRange(new string[] { "New", "In-Progress", "Resolved", "Closed" });
            cmbStatus.SelectedIndex = 0;
        }

        private void LoadTickets()
        {
            dgTickets.AutoGenerateColumns = true;
            dgTickets.DataSource = _ticketService.GetAll(null, null, null).ToList();
            dgTickets.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgTickets.ReadOnly = true;
            dgTickets.AllowUserToAddRows = false;
        }

        private void btnCreateTicket_Click(object sender, EventArgs e)
        {
            Model.Ticket ticket = new Model.Ticket()
            {
                IssueTitle = txtIssueTitle.Text,
                Description = txtDescription.Text,
                CategoryId = Convert.ToInt32(cmbCategory.SelectedValue),
                AssignedEmployeeId = Convert.ToInt32(cmbAssignedTo.SelectedValue),
                Status = cmbStatus.Text,
                ResolutionNotes = txtResolution.Text
            };

            var result = _ticketService.Add(ticket);

            if (!result.isOk)
                MessageBox.Show(result.message);

            if (result.isOk)
            {
                MessageBox.Show(result.message);
                LoadDefaultValues();
                LoadTickets();
                return;
            }
        }

        private void btnUpdateTicket_Click(object sender, EventArgs e)
        {
            if (dgTickets.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a ticket to update.");
                return;
            }

            int ticketId = Convert.ToInt32(
                dgTickets.SelectedRows[0].Cells["Id"].Value
            );

            Model.Ticket ticket = new Model.Ticket()
            {
                Id = ticketId,
                IssueTitle = txtIssueTitle.Text.Trim(),
                Description = txtDescription.Text.Trim(),
                CategoryId = Convert.ToInt32(cmbCategory.SelectedValue),
                AssignedEmployeeId = cmbAssignedTo.SelectedValue == null
                    ? 0
                    : Convert.ToInt32(cmbAssignedTo.SelectedValue),
                Status = cmbStatus.Text,
                ResolutionNotes = txtResolution.Text
            };

            var result = _ticketService.Update(ticket);

            if (!result.isOk)
            {
                MessageBox.Show(result.message, "Update Failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            MessageBox.Show(result.message, "Success",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            LoadTickets();
        }

        private void dgTickets_SelectionChanged(object sender, EventArgs e)
        {
            if (dgTickets.SelectedRows.Count == 0)
                return;

            var row = dgTickets.SelectedRows[0];

            txtIssueTitle.Text = row.Cells["IssueTitle"].Value?.ToString();
            txtDescription.Text = row.Cells["Description"].Value?.ToString();
            cmbStatus.Text = row.Cells["Status"].Value?.ToString();
            txtResolution.Text = row.Cells["ResolutionNotes"].Value?.ToString();

            cmbCategory.Text = row.Cells["Category"].Value?.ToString();


            cmbAssignedTo.Text = row.Cells["AssignedEmployee"].Value?.ToString();
        }

        private void btnDeleleteTicket_Click(object sender, EventArgs e)
        {
            if (!chkConfirmDelete.Checked)
            {
                MessageBox.Show(
                    "Please check the box to confirm deletion.",
                    "Confirmation Required",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            if (dgTickets.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a ticket to delete.");
                return;
            }

            int ticketId = Convert.ToInt32(dgTickets.SelectedRows[0].Cells["Id"].Value);


            var result = _ticketService.Delete(ticketId);

            if (!result.isOk)
            {
                MessageBox.Show(result.message, "Delete Failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }


            MessageBox.Show(result.message, "Deleted",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            chkConfirmDelete.Checked = false;
            LoadTickets();
        }
    }
}
