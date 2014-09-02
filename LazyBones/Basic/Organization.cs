using System;
using System.Collections.Generic;
using System.Linq;
using LazyBones.Basic.Data;

namespace LazyBones.Basic
{
    /// <summary>
    /// 表示一个组织层级结构
    /// </summary>
    public class Organization
    {
        Mediator mediator;
        internal Organization(Mediator mediator)
        {
            this.mediator = mediator;
            var departs = mediator.DepartmentManager.List();
            Load();
        }
        List<DepartWrapper> rootNodes = new List<DepartWrapper>();
        Dictionary<string, DepartWrapper> nodeDict = new Dictionary<string, DepartWrapper>();
        void Load() //加载数据，根据人员和部门信息自动生成组织层级结构
        {
            rootNodes.Clear();
            nodeDict.Clear();
            var cache = new List<Department>(mediator.DepartmentManager);
            LoadRootDepart(cache);
            AttachEmployee();
        }
        void LoadRootDepart(List<Department> cache)
        {
            foreach (var d in cache.Where(d => d.ParentDepart == null))
            {
                var node = new DepartWrapper();
                node.department = d;
                node.parent = null;
                rootNodes.Add(node);
                nodeDict[d.Id] = node;
            }
            cache.RemoveAll(d => d.ParentDepart == null);
            foreach (var node in rootNodes)
            {
                LoadDepart(node, cache);
            }
        }
        void LoadDepart(DepartWrapper node, List<Department> cache)
        {
            foreach (var d in cache.Where(d => d.ParentDepart == node.Department.Id))
            {
                var subNode = new DepartWrapper();
                subNode.department = d;
                subNode.parent = node;
                node.subDeparts.Add(subNode);
                nodeDict[d.Id] = subNode;
            }
            cache.RemoveAll(d => d.ParentDepart == node.Department.Id);
            foreach (var n in node.subDeparts)
            {
                LoadDepart(n, cache);
            }
        }
        void AttachEmployee()
        {
            foreach (var e in mediator.EmployeeManager)
            {
                AttachEmployee(e.FirstDepartment, e);
                if (e.SecondaryDeparts != null)
                    Array.ForEach(e.SecondaryDeparts, d => AttachEmployee(d, e));
            }
        }
        void AttachEmployee(string departId, Employee e)
        {
            DepartWrapper node;
            if (nodeDict.TryGetValue(e.FirstDepartment, out node))
            {
                node.employees.Add(e);
            }
        }
        /// <summary>
        /// 重新加载组织结构
        /// </summary>
        public void Reload()
        {
            Load();
        }
        /// <summary>
        /// 根据人员Id获取<see cref="Employee"/>对象
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        public Employee GetEmployee(string employeeId)
        {
            return mediator.EmployeeManager.FirstOrDefault(e => e.Id == employeeId);
        }
        /// <summary>
        /// 根据部门Id获取部门信息，不存在则返回null
        /// </summary>
        /// <param name="departId">部门Id</param>
        /// <returns>部门详细信息</returns>
        public DepartWrapper this[string departId]
        {
            get
            {
                DepartWrapper depart = null;
                nodeDict.TryGetValue(departId, out depart);
                return depart;
            }
        }
        /// <summary>
        /// 获取人员管理器
        /// </summary>
        public IManager<Employee> EmployeeManager
        {
            get { return mediator.EmployeeManager; }
        }
        /// <summary>
        /// 获取部门管理器
        /// </summary>
        public IManager<Department> DepartmentManager
        {
            get { return mediator.DepartmentManager; }
        }
        /// <summary>
        /// 获取顶级部门列表
        /// </summary>
        public DepartWrapper[] RootDeparts
        {
            get { return rootNodes.ToArray(); }
        }
    }
    /// <summary>
    /// 部门包裹器，用于提供比<see cref="Department"/>对象更加丰富的部门信息
    /// </summary>
    public class DepartWrapper
    {
        internal Department department;
        /// <summary>
        /// 获取对应的<see cref="Department"/>对象
        /// </summary>
        public Department Department
        {
            get { return department; }
        }
        internal List<DepartWrapper> subDeparts = new List<DepartWrapper>();
        /// <summary>
        /// 获取子部门列表，只读
        /// </summary>
        public IList<DepartWrapper> SubDeparts
        {
            get { return subDeparts.AsReadOnly(); }
        }
        internal DepartWrapper parent;
        /// <summary>
        /// 获取父级部门
        /// </summary>
        public DepartWrapper Parent
        {
            get { return parent; }
        }
        internal List<Employee> employees = new List<Employee>();
        /// <summary>
        /// 获取部门人员列表（不包括子部门），只读
        /// </summary>
        public IList<Employee> Employees
        {
            get { return employees.AsReadOnly(); }
        }
        /// <summary>
        /// 获取部门所有人员列表（包括所有子部门）
        /// </summary>
        public Employee[] AllEmployees
        {
            get
            {
                if (subDeparts.Count <= 0)
                    return employees.ToArray();
                return employees.Concat(subDeparts.SelectMany(c => c.AllEmployees)).Distinct().ToArray();
            }
        }
        /// <summary>
        /// 重写<see cref="object.ToString()"/>
        /// </summary>
        public override string ToString()
        {
            return department == null ? "" : department.ToString();
        }
    }
}
