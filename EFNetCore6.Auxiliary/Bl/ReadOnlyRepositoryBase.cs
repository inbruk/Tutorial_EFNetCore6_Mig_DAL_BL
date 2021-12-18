﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using EFNetCore6.Auxiliary.DAL;
using EFNetCore6.Auxiliary.Helpers;

namespace EFNetCore6.Auxiliary.BL
{
    /// <summary>
    /// Базовый класс для создания read only репозиториев бизнес логики
    /// </summary>
    /// <typeparam name="DTO">дто уровня бизнес логики</typeparam>
    /// <typeparam name="ENT">дто уровня доступа к данным - EF entity</typeparam>
    public class ReadOnlyRepositoryBase<DTO, ENT> : IReadOnlyRepository<DTO>
        where DTO : DTOBase
        where ENT : DTOBase
    {
        public int MaximumAcceptablePerformedRowsCount { get; protected set; }
        protected IMappingHelper? _mappingHelper = null;
        protected IUnitOfWork? _unitOfWork = null;
        public void Configure(IMappingHelper mh, IUnitOfWork uw, int maxRows)
        {
            _mappingHelper = mh;
            _unitOfWork = uw;
            MaximumAcceptablePerformedRowsCount = maxRows;
        }
        public void CheckPreset()
        {
            if (_mappingHelper == null) 
                throw new NullReferenceException("_mappingHelper");

            if (_unitOfWork == null)
                throw new NullReferenceException("_unitOfWork");
        }
        public void CheckPresetAndParams(object? param, string paramFullName, int? rowsCount)
        {
            CheckPreset();

            if (param == null)
                throw new ArgumentNullException(paramFullName);

            if (rowsCount!=null && rowsCount > MaximumAcceptablePerformedRowsCount)
                throw new Exception("Превышено максимальное количество строк, обрабатываемых за 1 запрос. См. MaximumAcceptablePerformedRowsCount");
        }
        public int GetAllCount()
        {
            int count = _unitOfWork.GetRepository<ENT>().GetAllCount();
            return count;
        }
        public DTO? Read(Guid id)
        {
            CheckPreset();
            var ent = _unitOfWork.GetRepository<ENT>().FirstOrDefault(x => x.Id == id);
            if (ent == null)
                return null;
            var dto = _mappingHelper.Map<ENT, DTO>(ent);
            return dto;
        }
        public List<DTO> ReadAll()
        {
            CheckPreset();
            var entList = _unitOfWork.GetRepository<ENT>().GetAll().ToList();
            var dtoList = _mappingHelper.Map<ENT, DTO>(entList);
            return dtoList;
        }
        public List<DTO> Read(List<Guid> idList)
        {
            int rowsCount = GetAllCount();
            CheckPresetAndParams(idList, @"ReadOnlyRepositoryBase.Read({nameof(idList)}", rowsCount);
            var entList = _unitOfWork.GetRepository<ENT>().FindBy( x => idList.Contains(x.Id) ).ToList();
            var dtoList = _mappingHelper.Map<ENT, DTO>(entList);
            return dtoList;
        }
    }
}